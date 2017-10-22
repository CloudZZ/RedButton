using System.Collections.Generic;
using System.Web.Mvc;
using RedButton.Models;
using RedButton.Services;

namespace RedButton.Controllers
{
    public class RedButtonController : Controller
    {
        private readonly OuManagerService _ouManager;
        private readonly MailService _mailService;
        private readonly ConnectionDropService _dropService;

        public RedButtonController(OuManagerService ouManager, MailService mailService, ConnectionDropService dropService)
        {
            _ouManager = ouManager;
            _mailService = mailService;
            _dropService = dropService;
        }

        [HttpGet]
        public ActionResult Test(string organization)
        {
            //var accounts = _ouManager.ListAllActiveAccounts(organization);
            var test = _dropService.GetActiveAccounts();
            return View(test);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Panic(PanicViewModel viewModel)
        {
            if(_ouManager.IsValidKey(viewModel.Organization, viewModel.Key))
            {
                var organizationMail = _ouManager.GetMail(viewModel.Organization);
                var disabledAccounts = _ouManager.DisableAccounts(viewModel.Organization);
                _mailService.SendMessage(organizationMail);
                _mailService.SendAdminMessage(disabledAccounts);
                _dropService.DropConnection(disabledAccounts);
                return RedirectToAction("Restore");
            }

            return View(viewModel);
        }
    }
}