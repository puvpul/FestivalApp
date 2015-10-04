using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazdaFestival.Controllers;
using SendGrid;
using System.Net;
using System.Net.Mail;


namespace MazdaFestival.Models
{
    class MazdaFestivalRepository
    {
        private MazdaFestivalEntities _db = new MazdaFestivalEntities();

        public int AddFestivalRegistration(UserRegistration content)
        {
            _db.UserRegistrations.Add(content);
            _db.SaveChanges();
            return content.ID;
            
        }

        public void RewardUser(UserRegistration referrer, UserRegistration referee, string root)
        {
            var mailMessage = new SendGridMessage();
            mailMessage.AddTo(referrer.Email);
            mailMessage.From = new MailAddress("no-reply@mazdathunderfestival.com.au");
            mailMessage.Subject = "A friend you've referred has registered!";

            string body = string.Empty;
            using (StreamReader reader = new StreamReader(String.Format(@"{0}/Email/RewardReferrerEmailTemplate.html", root)))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{referrer}", referrer.FirstName);
            body = body.Replace("{referee}", referee.FirstName);

            mailMessage.Html = body;

            var UserName = ConfigurationManager.AppSettings["mailAccount"];
            var Password = ConfigurationManager.AppSettings["mailPassword"];
            var Credentials = new NetworkCredential(UserName, Password);

            var transportWeb = new Web(Credentials);
            transportWeb.DeliverAsync(mailMessage);
        }

        public void AddReferedFriend(ReferedPerson rf, IDictionary<string, string> to)
        {
            _db.ReferedPersons.Add(rf);
            _db.SaveChanges();
            string RefpersonMail = rf.FriendEmail;
            string RefName = rf.FriendName;
            SendReferedMail(RefpersonMail, RefName, to["name"], to["email"]);
        }
        
        public void SendReferedMail(string frndMail, string frndName, string fromName, string fromEmail)
        {
            var mailMessage = new SendGridMessage();
            mailMessage.AddTo(frndMail);
            mailMessage.From = new MailAddress(fromEmail);
            mailMessage.Subject = "You've been invited to the Mazda Sydney Thunder Festival";

            string text = String.Format(@"
<p>Dear {0},</p>
<p>I’ve just entered the Mazda Sydney Thunder Festival which puts me in the draw to win some amazing prizes. Thought you might be interested in registering too.</p>
<p>It’s easy to get started. Just <a href=""http://www.mazdathunderfestival.com.au/?via={1}"">click here</a> to register and score your first entry into the  draw to win once-in-a-lifetime experiences and money- can’t-buy-prizes.</p>
<p>There’ll be lots of lucky winners drawn each week. Plus, registered festival members get the chance to take part in the online Mazda Mission game – which could see you take home a fantastic soon to be announced prize (stayed tuned)!</p>
<p>If you’re in it to win it, you can get bonus entries for referring friends, test driving a Mazda, and of course buying a car! Visit the Sydney Thunder Festival website to get amongst the action.</p>
<p>Good Luck,</p>
<p>{2}</p>", frndName, fromEmail, fromName);

            mailMessage.Html = text;

            var UserName = ConfigurationManager.AppSettings["mailAccount"];
            var Password = ConfigurationManager.AppSettings["mailPassword"];
            var Credentials = new NetworkCredential(UserName, Password);

            var transportWeb = new Web(Credentials);
            transportWeb.DeliverAsync(mailMessage);
        }

        public UserRegistration GetRegistrationById(int id)
        {
            UserRegistration ur = new UserRegistration();
            ur = _db.UserRegistrations.FirstOrDefault(d => d.ID == id);
            return ur;
        }

        public UserRegistration GetRegistrationByEmail(string email)
        {
            UserRegistration ur = new UserRegistration();
            ur = _db.UserRegistrations.FirstOrDefault(d => d.Email == email);
            return ur;
        }

        public IQueryable<Dealer> FindAllDealers()
        {
            return _db.Dealers;
        }

        public UniqueCode GetCodeByUserID(int id)
        {
           UniqueCode uc = new UniqueCode();
           uc = _db.UniqueCodes.FirstOrDefault(df => df.UserID == id);
            return uc;
        }
        public UserRegistration GetUserByEmail(string festUserEmail)
        {
            UserRegistration festivalUser = new UserRegistration();
            festivalUser =  (from m in _db.UserRegistrations
                             where m.Email.Equals(festUserEmail)
                             select m).Single();
            
            return festivalUser;
        }

        public void AddUniqueCode(int userId)
        {
            UniqueCode newCode = new UniqueCode();

            newCode.UserID = userId;
            newCode.ActivationCode = RandomString(6);
            _db.UniqueCodes.Add(newCode);
            _db.SaveChanges();
            
        }
        private string RandomString(int Size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);

            string input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789012345678901234567890123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < Size; i++)
            {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }
            return builder.ToString();
        }

        internal string GetCodeByCode(string festCode)
        {
            throw new NotImplementedException();
        }

        public void AddPoints(UserPoint rf)
        {
            _db.UserPoints.Add(rf);
            _db.SaveChanges();
        }

        public bool GetEmailByString(string em)
        {
            var retVal = _db.UserRegistrations.Any(r => r.Email.Equals(em));
            if (retVal)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
