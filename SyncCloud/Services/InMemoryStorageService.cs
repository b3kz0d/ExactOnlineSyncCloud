using DotNetOpenAuth.OAuth2;
using Dropbox.Api;
using ExactOnline.Client.Models.Current;
using SyncCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyncCloud.Services
{
    public class InMemoryStorageService : IStorageService
    {
        private List<AuthorizedUserModel> users = new List<AuthorizedUserModel>();
        private List<FileReferenceModel> fileReferences = new List<FileReferenceModel>();
        private Dictionary<string, string> lastCursors = new Dictionary<string, string>();

        public bool Add(AuthorizedUserModel user)
        {
            var _user = users.FirstOrDefault(x => x.CurrentMe != null && x.CurrentMe.UserID == user.CurrentMe.UserID);
            if (_user == null)
            {
                users.Add(user);
                return true;
            }
            return false;
        }

        public bool Update(AuthorizedUserModel user)
        {
            var _user = users.FirstOrDefault(x => x.CurrentMe != null && x.CurrentMe.UserID == user.CurrentMe.UserID);
            if (_user != null)
            {
                _user.ExactAuthorization = user.ExactAuthorization;
                _user.DropboxAuthorization = user.DropboxAuthorization;
                return true;
            }
            return false;
        }

        public AuthorizedUserModel GetUserByExactUserId(Guid id)
        {
            return users.FirstOrDefault(x => x.CurrentMe != null && x.CurrentMe.UserID == id);
        }

        public AuthorizedUserModel GetUserByDropboxUserId(string id)
        {
            return users.FirstOrDefault(x => x.DropboxAuthorization != null && x.DropboxAuthorization.Uid == id);
        }

        public void AddCursor(string dropboxUserId, string lastCursor)
        {
            if (!lastCursors.ContainsKey(dropboxUserId))
            {
                lastCursors.Add(dropboxUserId, lastCursor);
            }
            else
            {
                lastCursors[dropboxUserId] = lastCursor;
            }
        }

        public string GetCursor(string dropboxUserId)
        {
            if (lastCursors.ContainsKey(dropboxUserId))
            {
                return lastCursors[dropboxUserId];
            }
            return string.Empty;
        }

        public void AddFileReference(FileReferenceModel fileReferenceModel)
        {
            fileReferences.Add(fileReferenceModel);
        }

        public List<FileReferenceModel> GetAllFileReferences(Guid userID)
        {
            return fileReferences.Where(x => x.UserId == userID).ToList();
        }
    }
}