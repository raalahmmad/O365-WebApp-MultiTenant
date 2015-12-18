using Microsoft.IdentityModel.Clients.ActiveDirectory;
using O365_WebApp_MultiTenant.Models;
using O365_WebApp_MultiTenant.Utils;
using System.Web.Mvc;

namespace O365_WebApp_MultiTenant.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {

            var refreshToken = "AAABAAAAiL9Kn2Z27UubvWFPbm0gLZMlx6s4OJY-ikIdhhJyQZMRDu0UUQJHhznWghu_AED17dMa2154qRJq2Cw0IMIWAo5Uq_v-qq-lw41NBCAE2ZFKiZWzTg1cggOa76QvdN3ZDFE3dyr8ptE-TUr9pmvR3xX6uSpm9ksKWAhV-CSDmcS8gkaK7r_hiCAOxg1iTi2WELLgGV5j7xsuu-2PqAXfTVToekkyS4nS45Lh7e1RodPYrCk8Si3bF7nlp5gUNlcC_mYId0uJ3vS3CxB9IyF60qQkNawtk_aAEV-6mRw7SsjyzCK_pWIuxpO-N7Jj8klUmtJwHvSH3RUnlBArrBnlzlIKA15PzywCRlSPXRrnzlBmZnO3xoIIAefSzx8HkJ4tX9cIev-v2z-iMUqXynr-gZ18XwjAPeNkJ2XdX9bZAzPZoV30LV2Khd5vz3rkIs4wGArd1rsv__mM_ZYO4nOrwJ3Gfm0HrsHFRVKiEmTCoteu8f_KFDd7JF7W8BMi2yFDFekwfT2wVoMS6Uzw3BTQPFJKhi4Z8UKCR73lY0DIcah2TgzDFU57DNzZc4BgAv4ZaNfoeBaTAhDdxceC1QKoAttDyNU_goYJbFKNXE7iKb7OPWAXQtpEZIx0oSNrAXXSVwjg0oEmWyt9vKKEHeGboAELiYTbD7qJUeKJOUnUKSGf058EydA9j59Uy_CLaSV5tjObWNTFyhdE4d_iNmihxBQeiSBnliTI3ggNU7l5YcAgAA";
            ClientCredential credential = new ClientCredential(SettingsHelper.ClientId, SettingsHelper.AppKey);

            // AuthenticationContext authContext = new AuthenticationContext(string.Format("{0}/{1}", SettingsHelper.AuthorizationUri, tenantID), new ADALTokenCache(signInUserId));
            AuthenticationContext authContext = new AuthenticationContext(,)
            AuthenticationResult result2 = authContext.AcquireTokenByRefreshToken(refreshToken, credential);


            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}