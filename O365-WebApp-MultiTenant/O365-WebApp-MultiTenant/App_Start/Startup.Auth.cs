//----------------------------------------------------------------------------------------------
//    Copyright 2014 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;


using O365_WebApp_MultiTenant.Models;
using O365_WebApp_MultiTenant.Utils;
using Owin;
using System;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using System.Web;

namespace O365_WebApp_MultiTenant
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

                      
             
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = SettingsHelper.ClientId,
                    Authority = SettingsHelper.Authority,

                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps (single tenant apps)), 
                        // we turn off validation
                        //
                        // NOTE:
                        // * In a multitenant scenario you can never validate against a fixed issuer string, as every tenant will send a different one.
                        // * If you don’t care about validating tenants, as is the case for apps giving access to 1st party resources, you just turn off validation.
                        // * If you do care about validating tenants, think of the case in which your app sells access to premium content and you want to limit access only to the tenant that paid a fee, 
                        //       you still need to turn off the default validation but you do need to add logic that compares the incoming issuer to a list of tenants that paid you, 
                        //       and block access if that’s not the case.
                        // * Refer to the following sample for a custom validation logic: https://github.com/AzureADSamples/WebApp-WebAPI-MultiTenant-OpenIdConnect-DotNet

                        ValidateIssuer = false,
                        
                    },

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away. 
                        
                        AuthorizationCodeReceived = MyAuthorizationCodeReceived,                        
                        RedirectToIdentityProvider = RedirectToIdentityProvider,                  
                        AuthenticationFailed = AuthenticationFailed,

                        // My methods to catch extra events
                        MessageReceived = MessageReceived,
                        SecurityTokenReceived = SecurityTokenReceived,
                        SecurityTokenValidated = SecurityTokenValidated
                        // End My
                        
                    }

                });
        }




       public Task MyAuthorizationCodeReceived (Microsoft.Owin.Security.Notifications.AuthorizationCodeReceivedNotification context)
        {

            var refreshToken = "AAABAAAAiL9Kn2Z27UubvWFPbm0gLZMlx6s4OJY-ikIdhhJyQZMRDu0UUQJHhznWghu_AED17dMa2154qRJq2Cw0IMIWAo5Uq_v-qq-lw41NBCAE2ZFKiZWzTg1cggOa76QvdN3ZDFE3dyr8ptE-TUr9pmvR3xX6uSpm9ksKWAhV-CSDmcS8gkaK7r_hiCAOxg1iTi2WELLgGV5j7xsuu-2PqAXfTVToekkyS4nS45Lh7e1RodPYrCk8Si3bF7nlp5gUNlcC_mYId0uJ3vS3CxB9IyF60qQkNawtk_aAEV-6mRw7SsjyzCK_pWIuxpO-N7Jj8klUmtJwHvSH3RUnlBArrBnlzlIKA15PzywCRlSPXRrnzlBmZnO3xoIIAefSzx8HkJ4tX9cIev-v2z-iMUqXynr-gZ18XwjAPeNkJ2XdX9bZAzPZoV30LV2Khd5vz3rkIs4wGArd1rsv__mM_ZYO4nOrwJ3Gfm0HrsHFRVKiEmTCoteu8f_KFDd7JF7W8BMi2yFDFekwfT2wVoMS6Uzw3BTQPFJKhi4Z8UKCR73lY0DIcah2TgzDFU57DNzZc4BgAv4ZaNfoeBaTAhDdxceC1QKoAttDyNU_goYJbFKNXE7iKb7OPWAXQtpEZIx0oSNrAXXSVwjg0oEmWyt9vKKEHeGboAELiYTbD7qJUeKJOUnUKSGf058EydA9j59Uy_CLaSV5tjObWNTFyhdE4d_iNmihxBQeiSBnliTI3ggNU7l5YcAgAA";
            var code = context.Code;

            ClientCredential credential = new ClientCredential(SettingsHelper.ClientId, SettingsHelper.AppKey);
            string tenantID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string signInUserId = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;

            AuthenticationContext authContext = new AuthenticationContext(string.Format("{0}/{1}", SettingsHelper.AuthorizationUri, tenantID), new ADALTokenCache(signInUserId));

            // Get the access token for AAD Graph. Doing this will also initialize the token cache associated with the authentication context
            // In theory, you could acquire token for any service your application has access to here so that you can initialize the token cache
            //AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, SettingsHelper.AADGraphResourceId);
            AuthenticationResult result2 = authContext.AcquireTokenByRefreshToken(refreshToken, credential);

            return Task.FromResult(0);
        }

      public Task RedirectToIdentityProvider (Microsoft.Owin.Security.Notifications.RedirectToIdentityProviderNotification<Microsoft.IdentityModel.Protocols.OpenIdConnectMessage,OpenIdConnectAuthenticationOptions> context)
        {
            // This ensures that the address used for sign in and sign out is picked up dynamically from the request
            // this allows you to deploy your app (to Azure Web Sites, for example)without having to change settings
            // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
            context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;

            return Task.FromResult(0);
        }

      public Task  AuthenticationFailed (Microsoft.Owin.Security.Notifications.AuthenticationFailedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnectMessage,OpenIdConnectAuthenticationOptions> context)
        {
            // Suppress the exception if you don't want to see the error
            context.HandleResponse();
            return Task.FromResult(0);
        }     
                           
      public Task MessageReceived (Microsoft.Owin.Security.Notifications.MessageReceivedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnectMessage,OpenIdConnectAuthenticationOptions> context)
        {
            var abc = context.OwinContext;
            var handelResponse = context.HandledResponse;
            var options = context.Options;
            var OwinContext = context.OwinContext;
            var protocolMessage = context.ProtocolMessage;
            var request = context.Request;
            var response = context.Response;
            var skipped = context.Skipped;
            var state = context.State;
            return Task.FromResult(0);
        }
      

        public Task SecurityTokenReceived(Microsoft.Owin.Security.Notifications.SecurityTokenReceivedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnectMessage,OpenIdConnectAuthenticationOptions> context)
        {
            var HandeledResponse = context.HandledResponse;
            return Task.FromResult(0);
        }

      public Task SecurityTokenValidated(Microsoft.Owin.Security.Notifications.SecurityTokenValidatedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnectMessage,OpenIdConnectAuthenticationOptions> context)
        {
            var AuthenticationTicket = context.AuthenticationTicket;
            return Task.FromResult(0);
        }  

}

}
