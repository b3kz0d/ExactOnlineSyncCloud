using Newtonsoft.Json;
using SyncCloud.Models;
using SyncCloud.Services;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SyncCloud.Controllers
{
    public class DropboxController : Controller
    {
        private DropboxService _dropboxService;
        public DropboxController(DropboxService dropboxService)
        {
            _dropboxService = dropboxService;
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult WebHook(string challenge)
        {
            return Content(challenge);
        
        }
        [System.Web.Mvc.HttpPost]
        public ActionResult WebHook()
        {
            var signatureHeader = Request.Headers.GetValues("X-Dropbox-Signature");
            if (signatureHeader == null || !signatureHeader.Any())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var stream = new StreamReader(Request.InputStream);
            var content = stream.ReadToEnd();

            string signature = signatureHeader.FirstOrDefault();

            var isValid= ValidateSignature(content,signature);
            if (!isValid)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = JsonConvert.DeserializeObject<DropboxWebHookInputModel>(content);
            var task = Task.Run(() =>
            {
                NotifyChanges(model);
            });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void NotifyChanges(DropboxWebHookInputModel model)
        {
            _dropboxService.Sync(model);
        }

        private bool ValidateSignature(string content, string signature)
        {
            string appSecret = ConfigurationManager.AppSettings["Dropbox_ClientSecret"];
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret)))
            {
                if (!VerifySha256Hash(hmac, content, signature))
                    return false;
            }
            return true;
        }

        private string GetSha256Hash(HMACSHA256 sha256Hash, string input)
        {
            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var stringBuilder = new StringBuilder();
            foreach (byte t in data)
            {
                stringBuilder.Append(t.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        private bool VerifySha256Hash(HMACSHA256 sha256Hash, string input, string hash)
        {
            string hashOfInput = GetSha256Hash(sha256Hash, input);
            if (String.Compare(hashOfInput, hash, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            return false;
        }
    }
}
