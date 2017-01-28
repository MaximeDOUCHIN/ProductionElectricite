using MongoDB.Driver;
using Production_Electricite.Controllers.utils;
using Production_Electricite.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Production_Electricite.Controllers.centrale
{
    public class CentraleController : ApiController
    {
        protected IMongoCollection<Centrale> _collection;

        [HttpPut]
        [Route("centrale/create")]
        public HttpResponseMessage CreateCentrale(Centrale centrale)
        {
            LoginInfos loginInfos = new LoginInfos();
            if(!isSessionExpired())
            {
                if (ModelState.IsValid)
                {
                    _collection = new Connexion().getCollection<Centrale>("Centrale");
                    return create(centrale);
                }
                else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter"));
        }

        private HttpResponseMessage create(Centrale centrale)
        {
            //LoginInfos loginInfos = new LoginInfos();
            Centrale newCentrale = new Centrale();
            HttpResponseMessage response = new HttpResponseMessage();
            User user = getUserFromCookies();

            Centrale centraleDB = _collection.AsQueryable().FirstOrDefault(c =>
                c.nom == centrale.nom
                && c.userId == user._id);

            if (centraleDB == null)
            {
                newCentrale._id = Guid.NewGuid().ToString();
                newCentrale.version = 1;
                newCentrale.userId = user._id;
                newCentrale.nom = centrale.nom;
                newCentrale.type = centrale.type;
                newCentrale.capacite = centrale.capacite;
                newCentrale.stock = 0;
                newCentrale.lastModified = DateTime.Now;

                _collection.InsertOne(newCentrale);
                response.Content = new StringContent(newCentrale.nom + " a été créée.");
                response.StatusCode = HttpStatusCode.Created;
                return response;
            }
            else return Request.CreateErrorResponse(
                HttpStatusCode.BadRequest, 
                new HttpException("La centrale "+newCentrale.nom + " existe déjà pour le user "+ user.login +"."));
        }

        bool isSessionExpired()
        {
            if (Request == null) return true;
            DateTime timeout;
            DateTime.TryParse(Request.Headers.GetCookies()[0]["timeout"].Value, out timeout);
            return timeout < DateTime.Now;
            //Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter");
        }

        User getUserFromCookies()
        {
            User user = new User();
            string login = "";
            try { login = Request.Headers.GetCookies()[0]["login"].Value; }
            catch { login = ""; };

            user.login = login;
            return new LoginInfos().getUserFromRequest(user);
        }
    }
}
