using DotNetOpenAuth.OAuth2;
using ExactOnline.Client.Models.Current;
using ExactOnline.Client.Models.Documents;
using ExactOnline.Client.Sdk.Controllers;
using ExactOnline.Client.Sdk.Delegates;
using ExactOnline.Client.Sdk.Helpers;
using SyncCloud.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;

namespace SyncCloud.Services
{
    public class ExactOnlineService : WebServerClient
    {
        #region Fields

        private IAuthorizationState _authorization;
        private ExactOnlineClient _client=null;

        #endregion

        #region Properties

        public IAuthorizationState Authorization
        {
            get
            {
                return _authorization;
            }
        }

        #endregion

        #region Constructor

        public ExactOnlineService()
            : base(CreateAuthorizationServerDescription(), GetClientIdentifier(), GetClientSecret())
        {
            ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(GetClientSecret());
        }

        #endregion

        #region Public Methods

        public void Authorize(HttpSessionStateBase session)
        {
            var sessionId = ConfigurationManager.AppSettings["Exact_SessionId"];
            _authorization = (IAuthorizationState)session[sessionId];
            Authorize();
            session[sessionId] = _authorization;
            ConfigureApi();
        }

        public void Authorize(IAuthorizationState authorization)
        {
            _authorization = authorization;
            ConfigureApi();
        }

        public Me GetCurrentMe()
        {
            var _apiConnector = new ApiConnector(new AccessTokenManagerDelegate(() => _authorization.AccessToken), _client);
            var _exactOnlineApiUrl = GetBaseUrl() + "api/v1/";
            var conn = new ApiConnection(_apiConnector, _exactOnlineApiUrl + "current/Me");
            string response = conn.Get("");
            response = ApiResponseCleaner.GetJsonArray(response);
            var converter = new EntityConverter();
            var currentMe = converter.ConvertJsonArrayToObjectList<Me>(response);
            return currentMe.FirstOrDefault();

        }

        public void UploadDocument(string fileName, byte[] filecontent)
        {
            var categories = GetDocumentCategories(_client);

            Document doc = new Document()
            {
                Subject = fileName,
                DocumentDate = DateTime.Now.Date,
                Type = 55,
                Category = categories[0].ID,
            };

            _client.For<Document>().Insert(ref doc);

            DocumentAttachment docAttachment = new DocumentAttachment()
            {
                Attachment = filecontent,
                FileName = fileName,
                Document = doc.ID,

            };

            _client.For<DocumentAttachment>().Insert(ref docAttachment);
        }

        #endregion

        #region Private Methods

        private void ConfigureApi()
        {
            if (_authorization == null) return;
            _client = new ExactOnlineClient(GetBaseUrl(), new AccessTokenManagerDelegate(() => _authorization.AccessToken));
        }

        private void Authorize()
        {
            if (_authorization == null)
            {
                var httpRequestBase = new HttpRequestWrapper(HttpContext.Current.Request);
                _authorization = ProcessUserAuthorization();
                if (_authorization == null)
                {
                    RequestUserAuthorization(null, new Uri(GetReturnUrl()));
                }
            }
            else
            {
                if (AccessTokenHasToBeRefreshed())
                {
                    RefreshAuthorization(_authorization);
                }
            }
        }

        private new IAuthorizationState ProcessUserAuthorization(HttpRequestBase request = null)
        {
            var code = HttpContext.Current.Request.QueryString["code"];
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            var client = new HttpClient();
            var form = new Dictionary<string, string>
            {
                {"code", code},
                {"grant_type", "authorization_code"},
                {"client_id", GetClientIdentifier()},
                {"client_secret", GetClientSecret()},
                {"redirect_uri", GetReturnUrl()},
            };
            var content = new FormUrlEncodedContent(form);
           
            var tokenResponse = client.PostAsync(CreateAuthorizationServerDescription().TokenEndpoint, content).Result;
            var responseObject = tokenResponse.Content.ReadAsAsync<ExactOAuth2Response>(new[] { new JsonMediaTypeFormatter() }).Result;
            return GetAuthorizationState(responseObject);
        }

        private Boolean AccessTokenHasToBeRefreshed()
        {
            TimeSpan timeToExpire = _authorization.AccessTokenExpirationUtc.Value.Subtract(DateTime.UtcNow);
            return (timeToExpire.Minutes < 1);
        }

        private static string GetClientIdentifier()
        {
            return ConfigurationManager.AppSettings["Exact_ClientId"];
        }

        private static string GetClientSecret()
        {
            return ConfigurationManager.AppSettings["Exact_ClientSecret"];
        }

        private static string GetReturnUrl()
        {
            return ConfigurationManager.AppSettings["Exact_RedirectUrl"];
        }

        private static string GetBaseUrl()
        {
            var exactOnlineUrl = ConfigurationManager.AppSettings["Exact_BaseUrl"];
            if (!exactOnlineUrl.EndsWith("/")) exactOnlineUrl += "/";
            return exactOnlineUrl;
        }

        private static AuthorizationServerDescription CreateAuthorizationServerDescription()
        {
            var baseUri = GetBaseUrl();
            var uri = new Uri(baseUri.EndsWith("/") ? baseUri : baseUri + "/");
            var serverDescription = new AuthorizationServerDescription
            {
                AuthorizationEndpoint = new Uri(uri, "api/oauth2/auth"),
                TokenEndpoint = new Uri(uri, "api/oauth2/token")
            };
            return serverDescription;
        }

        private static string GetUrlRoot()
        {
            string port = HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
            port = port == null || port == "80" || port == "443" ? "" : ":" + port;

            string protocol = HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"];
            protocol = protocol == null || protocol == "0" ? "http://" : "https://";

            return protocol + HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + port + HttpContext.Current.Request.ApplicationPath;
        }

        private IAuthorizationState GetAuthorizationState(ExactOAuth2Response response)
        {
            IAuthorizationState authorizationState = new AuthorizationState()
            {
                AccessToken = response.access_token,
                RefreshToken = response.refresh_token,
                AccessTokenIssueDateUtc = DateTime.UtcNow,
                AccessTokenExpirationUtc = DateTime.UtcNow.AddSeconds(response.expires_in)
            };
            return authorizationState;
        }

        private List<DocumentCategory> GetDocumentCategories(ExactOnlineClient client)
        {
            var _apiConnector = new ApiConnector(new AccessTokenManagerDelegate(() => _authorization.AccessToken), client);
            var _exactOnlineApiUrl = GetBaseUrl() + "api/v1/";
            var conn = new ApiConnection(_apiConnector, _exactOnlineApiUrl + client.GetDivision().ToString() + "/documents/DocumentCategories");
            string response = conn.Get("");
            response = ApiResponseCleaner.GetJsonArray(response);
            var converter = new EntityConverter();
            var categories = converter.ConvertJsonArrayToObjectList<DocumentCategory>(response);
            return categories;
        }

        #endregion
    }
}