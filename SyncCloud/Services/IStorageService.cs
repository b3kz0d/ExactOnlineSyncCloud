using DotNetOpenAuth.OAuth2;
using Dropbox.Api;
using ExactOnline.Client.Models.Current;
using SyncCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncCloud.Services
{
    public interface IStorageService
    {
        bool Add(AuthorizedUserModel user);
        bool Update(AuthorizedUserModel user);
        AuthorizedUserModel GetUserByExactUserId(Guid id);
        AuthorizedUserModel GetUserByDropboxUserId(string id);
        void AddCursor(string dropboxUserId, string lastCursor);
        string GetCursor(string dropboxUserId);
        void AddFileReference(FileReferenceModel fileReferenceModel);
        List<FileReferenceModel> GetAllFileReferences(Guid userID);
    }
}
