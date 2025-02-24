using Microsoft.AspNetCore.Mvc;
using portfolio_backend.Services;
using portfolio_backend.Models;
using System.Threading.Tasks;
using System;

namespace portfolio_backend.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly GmailOAuth _gmailOAuth;
        private readonly GmailService _gmailService;

        public ContactController(GmailOAuth gmailOAuth)
        {
            _gmailOAuth = gmailOAuth;
            _gmailService = new GmailService(_gmailOAuth);
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail([FromBody] ContactModel form)
        {
            if (string.IsNullOrEmpty(form.Name) || string.IsNullOrEmpty(form.Email) || string.IsNullOrEmpty(form.Message))
                return BadRequest(new { message = "Invalid input" });

            try
            {
                var gmailService = await _gmailOAuth.GetGmailServiceAsync("authorization_code");

                string body = $@"
                <p><b>Name:</b> {form.Name}</p>
                <p><b>Email:</b> {form.Email}</p>
                <p><b>Message:</b></p>
                <p>{form.Message}</p>
                ";

                bool success = await _gmailService.SendEmailAsync(form.Email, body);

                if (success)
                    return Ok(new { message = "Email sent successfully" });

                return StatusCode(500, new { message = "Failed to send email" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
            }
        }

        [HttpGet("oauth2callback")]
        public async Task<IActionResult> OAuth2Callback(string code)
        {
            try
            {
                var gmailService = await _gmailOAuth.ExchangeCodeForToken(code);

                return Ok(new { message = "OAuth2 authentication successful", gmailService });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "OAuth2 authentication failed", error = ex.Message });
            }
        }
    }
}

