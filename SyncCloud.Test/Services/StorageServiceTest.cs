using System;
using DotNetOpenAuth.OAuth2;
using Dropbox.Api;
using ExactOnline.Client.Models.Current;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncCloud.Models;
using SyncCloud.Services;

namespace SyncCloud.Test.Services
{
    [TestClass]
    public class StorageServiceTest
    {
        [TestMethod]
        public void AuthorizedUserTest()
        {
            var authorizedUser = new AuthorizedUserModel();
            authorizedUser.CurrentMe = new Me { UserID = Guid.NewGuid() };
            authorizedUser.ExactAuthorization = new AuthorizationState() { AccessToken = "aabbcc" };

            var storageService = new InMemoryStorageService();
            storageService.Add(authorizedUser);
            var result=storageService.GetUserByExactUserId(authorizedUser.CurrentMe.UserID);
            Assert.AreSame(authorizedUser, result);

            authorizedUser.ExactAuthorization.AccessToken = "ddeeff";
            var updatedResult = storageService.Update(authorizedUser);
            Assert.IsTrue(updatedResult);
        }

        [TestMethod]
        public void FileReferenceTest()
        {
            var fileReference = new FileReferenceModel
            {
                UserId=Guid.NewGuid(),
                UserName="FirstName LastName",
                FileId="1111",
                FileName="Reciept",
                FileSize=900,
                FilePath="/Reciept.pdf"
            };

            var storageService = new InMemoryStorageService();
            storageService.AddFileReference(fileReference);
            var result = storageService.GetAllFileReferences(fileReference.UserId);
            Assert.AreSame(fileReference, result[0]);

            fileReference.FileId = "2222";
            storageService.AddFileReference(fileReference);
            var updatedResult = storageService.GetAllFileReferences(fileReference.UserId);
            Assert.AreNotSame(updatedResult, result);
        }
    }
}
