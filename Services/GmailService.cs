using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using MimeKit;
using System;
using System.Text;
using System.Threading.Tasks;

namespace portfolio_backend.Services
{
    public class GmailService
    {
        private readonly Google.Apis.Gmail.v1.GmailService _gmailService;

        public GmailService(GmailOAuth gmailOAuth)
        {
            _gmailService = gmailOAuth.GetGmailServiceAsync("authorization_code").Result;
        }

        public async Task<bool> SendEmailAsync(string to, string body)
        {
            try
            {
                var message = new Message
                {
                    Raw = Base64UrlEncode(CreateMimeMessage(to, body))
                };

                var request = _gmailService.Users.Messages.Send(message, "me");
                var response = await request.ExecuteAsync();

                return response != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        private MimeMessage CreateMimeMessage(string to, string body)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Portfolio Website", "charlottemgaledev@gmail.com"));
            mimeMessage.To.Add(new MailboxAddress(null, to)); 
            mimeMessage.Subject = "New Contact Form Submission";
            mimeMessage.Body = new TextPart("html") { Text = body };
            return mimeMessage;
        }

        private string Base64UrlEncode(MimeMessage mimeMessage)
        {
            var rawMessage = mimeMessage.ToString();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawMessage))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}
