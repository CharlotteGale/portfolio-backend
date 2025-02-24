using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace portfolio_backend.Services
{
    public class GmailOAuth
    {
        private readonly IConfiguration _configuration;

        public GmailOAuth(IConfiguration configuration) => _configuration = configuration;

        public async Task<Google.Apis.Gmail.v1.GmailService> GetGmailServiceAsync(string code = null)
        {
            var clientId = _configuration["Gmail:ClientId"];
            var clientSecret = _configuration["Gmail:ClientSecret"];
            var credentialFilePath = _configuration["Gmail:CredentialFilePath"];

            var credentials = GoogleClientSecrets.FromFile(credentialFilePath).Secrets;

            UserCredential credential;

            var credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/gmail-dotnet.json");

            // testing environment uri = "http://localhost:5000/oauth2callback"
            // deployment environment uri = "https://charlottegale.dev/oauth2callback"

            if (!string.IsNullOrEmpty(code))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    credentials,
                    new[] { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                );
            }
            else
            {
                using var stream = new FileStream(credentialFilePath, FileMode.Open, FileAccess.Read);
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    credentials,
                    new[] { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                );
            }

            return new Google.Apis.Gmail.v1.GmailService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _configuration["Gmail:ApplicationName"]
            });
        }

        public async Task<UserCredential> ExchangeCodeForToken(string code)
        {
            var clientId = _configuration["Gmail:ClientId"];
            var clientSecret = _configuration["Gmail:ClientSecret"];
            var redirectUri = "https://charlottegale.dev/oauth2callback";

            var initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            };

            var flow = new GoogleAuthorizationCodeFlow(initializer);

            var tokenResponse = await flow.ExchangeCodeForTokenAsync("user", code, redirectUri, CancellationToken.None);

            return new UserCredential(flow, "user", tokenResponse);
        }
    }
}
