using DotNetOpenAuth.OAuth2;
using Dropbox.Api;
using ExactOnline.Client.Models.Current;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyncCloud.Models
{
    public class AuthorizedUserModel
    {
        public IAuthorizationState ExactAuthorization { get; set; }
        public OAuth2Response DropboxAuthorization { get; set; }
        public Me CurrentMe { get; set; }
    }
}