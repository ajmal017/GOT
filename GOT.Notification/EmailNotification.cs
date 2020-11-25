using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using GOT.SharedKernel;

namespace GOT.Notification
{
    public class EmailNotification : INotification
    {
        private const string HOST = "smtp.gmail.com";
        private const int PORT = 587;
        private const string USER_NAME = "bffnotify@gmail.com";
        private const string PASSWORD = "#6[qgutdX3L4Ps4";
        private const string HEADER = "Notify";
        private string _email;

        public EmailNotification(IConfiguration configuration)
        {
            _email = configuration.Email;
        }

        public void UpdateServiceInfo(IConfiguration configuration)
        {
            _email = configuration.Email;
        }

        public async Task SendMessageAsync(string body, string subject = "")
        {
            if (string.IsNullOrEmpty(_email)) 
                return;
            using var smtpClient = new SmtpClient(HOST, PORT)
            {
                EnableSsl = true, Credentials = new NetworkCredential(USER_NAME, PASSWORD)
            };
            var from = new MailAddress(USER_NAME, HEADER);
            var to = new MailAddress(_email);
            var message = new MailMessage(from, to)
            {
                Subject = subject, Body = body
            };

            await smtpClient.SendMailAsync(message);
        }

        public void SendMessage(string body, string subject = "")
        {
            if (string.IsNullOrEmpty(_email))
                return;
            using var smtpClient = new SmtpClient(HOST, PORT)
            {
                EnableSsl = true, Credentials = new NetworkCredential(USER_NAME, PASSWORD)
            };
            var from = new MailAddress(USER_NAME, HEADER);
            var to = new MailAddress(_email);
            var message = new MailMessage(from, to) {Subject = subject, Body = body};
            smtpClient.Send(message);
        }
    }
}