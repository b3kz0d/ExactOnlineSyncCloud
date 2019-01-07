using SyncCloud.Models;
using SyncCloud.Services;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;

namespace SyncCloud.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        #region Fields

        private IStorageService _storageService;
        private ExactOnlineService _exactOnlineService;
        private DropboxService _dropboxService;

        #endregion

        #region Constructor

        public HomeController(IStorageService storageService, ExactOnlineService exactOnlineService, DropboxService dropboxService)
        {
            _storageService = storageService;
            _exactOnlineService = exactOnlineService;
            _dropboxService = dropboxService;
        }

        #endregion

        #region Action Methods

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            ViewBag.IsDropboxAuthorized = false;
            _exactOnlineService.Authorize(HttpContext.Session);
            if (_exactOnlineService.Authorization == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            var me = _exactOnlineService.GetCurrentMe();
            var userAuthorization = _storageService.GetUserByExactUserId(me.UserID);
            var _userAuthorization = new AuthorizedUserModel { CurrentMe = me, ExactAuthorization = _exactOnlineService.Authorization };
            if (userAuthorization == null)
            {
                _storageService.Add(_userAuthorization);
            }
            else if (userAuthorization.DropboxAuthorization != null)
            {
                ViewBag.IsDropboxAuthorized = true;
            }

            ViewBag.UserName = me.FullName;

            List<FileReferenceModel> fileReferences=_storageService.GetAllFileReferences(me.UserID);
            return View(fileReferences);
        }

        public ActionResult Dropbox()
        {
            _exactOnlineService.Authorize(HttpContext.Session);
            _dropboxService.Authorize(HttpContext.Session);
            if (_dropboxService.Authorization == null|| _exactOnlineService.Authorization==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            var me = _exactOnlineService.GetCurrentMe();
            var userAuthorization = _storageService.GetUserByExactUserId(me.UserID);
            var _userAuthorization = new AuthorizedUserModel { CurrentMe = me, ExactAuthorization = _exactOnlineService.Authorization, DropboxAuthorization = _dropboxService.Authorization };
            if (userAuthorization == null)
            {
                _storageService.Add(_userAuthorization);
            }
            else
            {
                _storageService.Update(_userAuthorization);
            }

            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
