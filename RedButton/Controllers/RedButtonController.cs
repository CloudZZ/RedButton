using System.Web.Mvc;
using RedButton.Models;
using RedButton.Services;

namespace RedButton.Controllers
{
    public class RedButtonController : Controller
    {
        private readonly OuManagerService _ouManager;

        public RedButtonController(OuManagerService ouManager)
        {
            _ouManager = ouManager;
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Message = _ouManager.GetOuConnectionString("nodeart");
            return View();
        }

        [HttpGet]
        public ActionResult Restore()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Panic(string organization, string key)
        {
            if(_ouManager.IsValidKey(organization, key))
            {
                return View(new PanicViewModel { Organization = organization, Key = key });
            }

            return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Members(string organization, string key)
        {
            return View(new MembersViewModel { Members = _ouManager.ListAllActiveAccounts(organization) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Panic(PanicViewModel viewModel)
        {
            if(_ouManager.IsValidKey(viewModel.Organization, viewModel.Key))
            {
                _ouManager.DisableAccounts(viewModel.Organization);
                return RedirectToAction("Restore");
            }

            return View(viewModel);
        }
    }
}