using Newtonsoft.Json;
using SyncCloud.Helpers;
using SyncCloud.Models;
using SyncCloud.Services;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SyncCloud.Controllers
{
    public class DropboxController : Controller
    {
        #region Fields

        private DropboxService _dropboxService;

        #endregion

        #region Constructor

        public DropboxController(DropboxService dropboxService)
        {
            _dropboxService = dropboxService;
        }

        #endregion

        #region Action Methods

        [HttpGet]
        public ActionResult WebHook(string challenge)
        {
            return Content(challenge);
        
        }
        [HttpPost]
        public ActionResult WebHook()
        {
            var signatureHeader = Request.Headers.GetValues("X-Dropbox-Signature");
            if (signatureHeader == null || !signatureHeader.Any())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var stream = new StreamReader(Request.InputStream);
            var content = stream.ReadToEnd();

            string signature = signatureHeader.FirstOrDefault();

            var isValid= Helper.ValidateSignature(content,signature);
            if (!isValid)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = JsonConvert.DeserializeObject<DropboxWebHookInputModel>(content);
            var task = Task.Run(() =>
            {
                _dropboxService.Sync(model);
            });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        #endregion
    }
}
