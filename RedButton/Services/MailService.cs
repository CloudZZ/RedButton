using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace RedButton.Services
{
    public class MailService
    {
        private readonly SmtpClient _client;
        private readonly MailAddress _senderMail;
        private readonly MailAddress _adminMail;

        private readonly string _mailSubject;

        public MailService()
        {
            var username = ConfigurationManager.AppSettings["mail-username"];
            var password = ConfigurationManager.AppSettings["mail-password"];

            _client = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["mail-host"],
                Port = int.Parse(ConfigurationManager.AppSettings["mail-port"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            };

            _senderMail = new MailAddress(username);
            _adminMail = new MailAddress(ConfigurationManager.AppSettings["mail-admin-username"]);

            _mailSubject = ConfigurationManager.AppSettings["mail-subject"];
        }

        public void SendMessage(string receiver)
        {
            var msg = new MailMessage
            {
                From = _senderMail,
                Subject = _mailSubject,
                IsBodyHtml = true,
                Body = GetUserMessage()
            };
            msg.To.Add(new MailAddress(receiver));
            
            try
            {
                _client.Send(msg);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void SendAdminMessage(List<string> disabledAccounts)
        {
            var msg = new MailMessage
            {
                From = _senderMail,
                Subject = _mailSubject,
                IsBodyHtml = true,
                Body = GetAdminMessage(disabledAccounts)
            };
            msg.To.Add(_adminMail);

            try
            {
                _client.Send(msg);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static string GetUserMessage()
        {
            return "<html><head></head><body><h1>Red Button Alert</h1><p>Somebody pushed redbutton. All current connections are dropped, and future connections will be rejected till the end of alert</p></body>";
        }

        private static string GetAdminMessage(IReadOnlyCollection<string> accounts)
        {
            var message = new StringBuilder();
            message.Append("<html><head></head><body>");
            if (accounts.Count == 0)
            {
                message.Append("No accounts disabled");
            }
            else
            {
                message.Append("<h1>Disabled accounts:</h1><ul>");
                foreach (var account in accounts)
                {
                    message.Append($"<li>{account}</li>");
                }
                message.Append("</ul>");
            }
            message.Append("</body></html>");
            return message.ToString();
        }
    }
}