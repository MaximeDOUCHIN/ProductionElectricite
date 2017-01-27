using System;
using System.Net.Http;
using System.Web.Http;

namespace Production_Electricite.Controllers.utils
{
    public class Login : ApiController
    {
        public bool isSessionExpired()
        {
            DateTime timeout;
            DateTime.TryParse(Request.Headers.GetCookies()[0]["timeout"].Value, out timeout);
            return timeout > DateTime.Now;
            //Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter");
        }
    }
}