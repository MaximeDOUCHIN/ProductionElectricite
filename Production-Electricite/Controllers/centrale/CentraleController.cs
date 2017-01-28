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
        public HttpResponseMessage CreateCentrale(Centrale centraleRequest)
        {
            LoginInfos loginInfos = new LoginInfos();
            if (!isSessionExpired())
            {
                if (ModelState.IsValid)
                {
                    _collection = new utils.MongoDB().getCollection<Centrale>("Centrale");
                    return create(centraleRequest);
                }
                else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter."));
        }

        [HttpGet]
        [Route("centrale/{centrale}")]
        public HttpResponseMessage ConsulterStock(string centrale)
        {
            if (!isSessionExpired())
            {
                HttpResponseMessage response = new HttpResponseMessage();
                User user = getUserFromCookies();

                Centrale centraleDB = new utils.MongoDB().
                    getCollection<Centrale>("Centrale").
                    AsQueryable().
                    FirstOrDefault(c => c.userId == user._id && c.reference == centrale);

                if (centraleDB == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, new Exception("La centrale "+ centrale+" n'a pas été trouvée."));

                string nom = centraleDB.reference;
                double stock = centraleDB.stock;
                double capacite = centraleDB.capacite;
                double pourcentageOccupation = 100 * stock / capacite;

                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent("Stock de la centrale " + nom + " : \n\t" +
                    stock + " KW / " + capacite + " KW (" + pourcentageOccupation + " % utilisés)");

                return response;
            }
            else return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter"));
        }

        private HttpResponseMessage create(Centrale centraleRequest)
        {
            //LoginInfos loginInfos = new LoginInfos();
            Centrale newCentrale = new Centrale();
            HttpResponseMessage response = new HttpResponseMessage();
            User user = getUserFromCookies();

            Centrale centraleDB = _collection.AsQueryable().FirstOrDefault(c =>
                c.reference == centraleRequest.reference
                && c.userId == user._id);

            if (centraleDB == null)
            {
                newCentrale._id = Guid.NewGuid().ToString();
                newCentrale.version = 1;
                newCentrale.userId = user._id;
                newCentrale.reference = centraleRequest.reference;
                newCentrale.type = centraleRequest.type;
                newCentrale.capacite = centraleRequest.capacite;
                newCentrale.stock = 0;
                newCentrale.lastModified = DateTime.Now;

                _collection.InsertOne(newCentrale);
                response.Content = new StringContent(newCentrale.reference + " a été créée.");
                response.StatusCode = HttpStatusCode.Created;
                return response;
            }
            else return Request.CreateErrorResponse(
                HttpStatusCode.BadRequest,
                new HttpException("La centrale " + centraleRequest.reference + " existe déjà pour le user " + user.login + "."));
        }

        bool isSessionExpired()
        {
            if (Request == null || Request.Headers.GetCookies().Count == 0) return true;
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
