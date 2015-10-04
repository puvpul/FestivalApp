using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using SendGrid;
using System.Web.Mvc;
using MazdaFestival.Models;

namespace MazdaFestival.Controllers
{
    public class Submission
    {
        public IDictionary<string, string> from { get; set; }
        public ReferedPerson[] to { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly MazdaFestivalRepository _db = new MazdaFestivalRepository();

      

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Registration()
        {
            return PartialView("_Registration");
        }

        [HttpPost]
        public ActionResult Registration(UserRegistration regoModel)
        {
            int userId = this._db.AddFestivalRegistration(regoModel);
            UserRegistration newUser = new UserRegistration();
            //int userId = _db.GetLastInsertValue();
            newUser = _db.GetRegistrationById(userId);
            
            string usrMail = newUser.Email;

            if (userId != 0)
            {
                this.AddActivationCode(userId);
                this.SendActivationEmail(userId, usrMail);

                if (newUser.Search.StartsWith("?via="))
                {
                    string via = newUser.Search.Substring(5);
                    UserRegistration referrer = _db.GetRegistrationByEmail(via);
                    _db.RewardUser(referrer, newUser, Server.MapPath("~"));
                }

                return Json(new { success = "Registered Success and mail sent" });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

          
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult ReferFriend()
        {
            return PartialView("_referFriend");
        }
        
        public ActionResult GetRefererByEmail(string refEmail)
        {
            UserRegistration ur = new UserRegistration();
            var usrDetails = _db.GetUserByEmail(refEmail);
            ur.ID = usrDetails.ID;
           

            return Json(ur, JsonRequestBehavior.AllowGet);
        }

      
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ReferFriend(Submission data)
        {
            ReferedPerson[] referedList = data.to;

            foreach (ReferedPerson r in data.to) {
                _db.AddReferedFriend(r, data.from);
            }

            return Json(new { success = "Registered Success and mail sent" });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Recover(string email)
        {
            UserRegistration u = _db.GetRegistrationByEmail(email);
            SendActivationEmail(u.ID, u.Email);
            return Json("true");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult CheckEmail(string em)
        {
           bool unc = _db.GetEmailByString(em);
            if (unc)
            {
                return Json("true");
            }
            else
            {
                return Json("false");
            }
            
        }

        public ActionResult GetAllDealers()
        {
            return Json(_db.FindAllDealers(), JsonRequestBehavior.AllowGet);
        }

        public void AddActivationCode(int userId)
        {
          _db.AddUniqueCode(userId);
          
        }

        public void SendActivationEmail(int id, string mailTo)
        {
            UniqueCode usrCode = new UniqueCode();
            usrCode = _db.GetCodeByUserID(id);
            string usrUniqueCode = usrCode.ActivationCode;
            UserRegistration uName = new UserRegistration();
            uName = _db.GetRegistrationById(id);
            string userName = uName.FirstName;
          
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Email/WelcomeEmailTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{RegisteredUSR}", userName);
            body = body.Replace("{UserCode}", usrUniqueCode);

            this.SendFormattedEmail(mailTo, body);
        }

        public void SendFormattedEmail(string mailTo, string body)
        {
            
               var UserName = ConfigurationManager.AppSettings["mailAccount"];
                var Password = ConfigurationManager.AppSettings["mailPassword"];
               // smtp.UseDefaultCredentials = true;
                var Credentials = new NetworkCredential(UserName, Password);
                
                var mailMessage = new SendGridMessage();
                mailMessage.AddTo(mailTo);
                mailMessage.From = new MailAddress("no-reply@mazdathunderfestival.com.au");
                mailMessage.Subject = "Welcome to the Mazda Sydney Thunder Festival";
                mailMessage.Html = body;
                var transportWeb = new Web(Credentials);
                transportWeb.DeliverAsync(mailMessage);

            // smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            // smtp.Send(mailMessage);

        }

        public ActionResult ResendCode()
        {
            return PartialView("_resendCode");
        }

        public ActionResult SubmitPoints()
        {
            return PartialView("_submitPoints");
        }

        public ActionResult ReferFriends()
        {
            return PartialView("_referFriend");
        }

        public ActionResult TermsAndConditions()
        {
            return PartialView("_termsAndConditions");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SubmitPoints(UserPoint up)
        {
            _db.AddPoints(up);
            return Json(new { success = "Registered Success and mail sent" });
        }

     }
}