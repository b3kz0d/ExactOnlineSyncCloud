using SyncCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dropbox.Api;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using System.Net.Http;
using System.IO;
using DotNetOpenAuth.OAuth2;

namespace SyncCloud.Services
{
    public class DropboxService
    {
        #region Fields

        private OAuth2Response _authorization;
        private IStorageService _storageService;
        private ExactOnlineService _exactOnlineService;
        DropboxClient _client;

        #endregion

        #region Properties

        public OAuth2Response Authorization
        {
            get
            {
                return _authorization;
            }
            set
            {
                _authorization = value;
            }
        }

        #endregion

        #region Constructor

        public DropboxService(IStorageService storageService, ExactOnlineService exactOnlineService)
        {
            _storageService = storageService;
            _exactOnlineService = exactOnlineService;
            ConfigureApi();
        }

        #endregion

        #region Public Methods

        public void Authorize(HttpSessionStateBase session)
        {
            var sessionId = ConfigurationManager.AppSettings["Dropbox_SessionId"];
            _authorization = (OAuth2Response)session[sessionId];
            Authorize();
            session[sessionId] = _authorization;
            ConfigureApi();
        }

        public void Authorize(OAuth2Response authorization)
        {
            _authorization = authorization;
            ConfigureApi();
        }

        public async void Sync(DropboxWebHookInputModel model)
        {
            var userAuthorization = _storageService.GetUserByDropboxUserId(model.Delta.Users[0].ToString());
            if (userAuthorization == null) return;

            bool hasMore = true;

            this.Authorize(userAuthorization.DropboxAuthorization);
            _exactOnlineService.Authorize(userAuthorization.ExactAuthorization);
            
            var lastCursor = _storageService.GetCursor(_authorization.Uid);
            ListFolderResult list = await _client.Files.ListFolderContinueAsync(lastCursor);
            
            while (hasMore)
            {
                foreach (var entry in list.Entries)
                {
                    if (entry.IsDeleted || entry.IsFolder)
                        continue;
                    
                    var fileContent= await Download(_client, "", entry.AsFile);

                    _exactOnlineService.UploadDocument(entry.Name, fileContent);

                    var fileReferenceModel = new FileReferenceModel
                    {
                        UserId = userAuthorization.CurrentMe.UserID,
                        UserName = userAuthorization.CurrentMe.FullName,
                        FileId = entry.AsFile.Id,
                        FileName = entry.Name,
                        FileSize = entry.AsFile.Size,
                        FilePath = entry.PathDisplay,
                        CreatedDate = DateTime.Now
                    };

                    _storageService.AddFileReference(fileReferenceModel);
                }

                lastCursor = list.Cursor;
                hasMore = list.HasMore;
                _storageService.AddCursor(_authorization.Uid, lastCursor);
            }
        }

        #endregion

        #region Private Mathods

        private void ConfigureApi()
        {
            if (_authorization == null) return;

            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };

            var config = new DropboxClientConfig("SyncCloudApp")
            {
                HttpClient = httpClient
            };

            _client = new DropboxClient(_authorization.AccessToken, config);
            var cursor = _storageService.GetCursor(_authorization.Uid);
            if (string.IsNullOrEmpty(cursor))
            {
                cursor = _client.Files.ListFolderGetLatestCursorAsync("").Result.Cursor;
                _storageService.AddCursor(_authorization.Uid, cursor);
            }
        }

        private void Authorize()
        {
            if (_authorization == null)
            {
                _authorization = ProcessUserAuthorization();
                if (_authorization == null)
                {
                    RequestUserAuthorization();
                }
            }
        }

        private void RequestUserAuthorization()
        {
            var uri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, GetClientIdentifier(), GetReturnUrl());
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            HttpContext.Current.Response.Redirect(uri.AbsoluteUri);
            HttpContext.Current.Response.End();
        }

        private OAuth2Response ProcessUserAuthorization()
        {
            var code = HttpContext.Current.Request.QueryString["code"];
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            var dropboxAccessToken = DropboxOAuth2Helper.ProcessCodeFlowAsync(code, GetClientIdentifier(), GetClientSecret(), GetReturnUrl()).Result;
            return dropboxAccessToken;
        }

        private static string GetClientIdentifier()
        {
            return ConfigurationManager.AppSettings["Dropbox_ClientId"];
        }

        private static string GetClientSecret()
        {
            return ConfigurationManager.AppSettings["Dropbox_ClientSecret"];
        }

        private static string GetReturnUrl()
        {
            return ConfigurationManager.AppSettings["Dropbox_RedirectUrl"];
        }
        private async Task<byte[]> Download(DropboxClient client, string folder, FileMetadata file)
        {
            var response = await client.Files.DownloadAsync(folder + "/" + file.Name);
            return await response.GetContentAsByteArrayAsync();
        }

        #endregion
    }
}