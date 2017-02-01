using MongoDB.Driver;
using Production_Electricite.Controllers.utils;
using Production_Electricite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Production_Electricite.Controllers.centrale
{
    public class CentraleController : ApiController
    {
        protected IMongoCollection<Centrale> _centrale;
        protected IMongoCollection<Stock> _stock;

        [HttpPut]
        [Route("centrale/creer")]
        public HttpResponseMessage CreateCentrale(Centrale centraleRequest)
        {
            LoginInfos loginInfos = new LoginInfos();
            if (!isSessionExpired())
            {
                if (ModelState.IsValid)
                {
                    _centrale = new utils.MongoDB().getCollection<Centrale>("Centrale");
                    _stock = new utils.MongoDB().getCollection<Stock>("Stock");

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
                _centrale = new utils.MongoDB().getCollection<Centrale>("Centrale");
                _stock = new utils.MongoDB().getCollection<Stock>("Stock");

                HttpResponseMessage response = new HttpResponseMessage();
                User user = getUserFromCookies();

                Centrale centraleDB = _centrale.AsQueryable().
                    FirstOrDefault(c => c.userId == user._id && c.reference == centrale);
                if (centraleDB == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, new Exception("La centrale " + centrale + " n'a pas été trouvée."));

                Stock stockDB = _stock.AsQueryable().OrderByDescending(s => s.dateCreation).First(s => s.idCentrale == centraleDB._id);
                if (stockDB == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception("La base de données est corrompue. Le stock de la centrale " + centrale + " est introuvable."));

                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent(stock(centraleDB, stockDB));
                return response;
            }
            else return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter"));
        }

        [HttpGet]
        [Route("centrale/{centrale}/historique")]
        public HttpResponseMessage ConsulterHistorique(string centrale)
        {
            if (!isSessionExpired())
            {
                _centrale = new utils.MongoDB().getCollection<Centrale>("Centrale");
                _stock = new utils.MongoDB().getCollection<Stock>("Stock");

                HttpResponseMessage response = new HttpResponseMessage();
                User user = getUserFromCookies();

                Centrale centraleDB = _centrale.AsQueryable().
                  FirstOrDefault(c => c.userId == user._id && c.reference == centrale);
                if (centraleDB == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, new Exception("La centrale " + centrale + " n'a pas été trouvée."));

                List<Stock> stocksDB = _stock.AsQueryable().OrderByDescending(s => s.dateCreation).Where(s => s.idCentrale == centraleDB._id).ToList();
                if (stocksDB == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception("La base de données est corrompue. L'historique du stock de la centrale " + centrale + " est introuvable."));

                string historique = "Historique du stock de la centrale "+ centrale + " :\n";

                foreach(Stock stock in stocksDB)
                {
                    historique += stock.dateCreation + " - " + stock.quantite + " KW\n";
                }
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent(historique);

                return response;
            }
            else return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter"));
        }

        [HttpPut]
        [Route("centrale/consommer")]
        public HttpResponseMessage ConsommerStock(Usage consumption)
        {

            if (!isSessionExpired())
            {
                if (ModelState.IsValid)
                {
                    _centrale = new utils.MongoDB().getCollection<Centrale>("Centrale");
                    _stock = new utils.MongoDB().getCollection<Stock>("Stock");

                    return consume(consumption);
                }
                else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter"));
        }


        [HttpPut]
        [Route("centrale/recharger")]
        public HttpResponseMessage RechargerStock(Usage refill)
        {
            if (!isSessionExpired())
            {
                if (ModelState.IsValid)
                {
                    _centrale = new utils.MongoDB().getCollection<Centrale>("Centrale");
                    _stock = new utils.MongoDB().getCollection<Stock>("Stock");

                    return this.refill(refill);
                }
                else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Session expirée, merci de vous reconnecter")); 
        }

        private HttpResponseMessage create(Centrale centraleRequest)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            User user = getUserFromCookies();

            Centrale centraleDB = _centrale.AsQueryable().FirstOrDefault(c =>
                c.reference == centraleRequest.reference
                && c.userId == user._id);

            if (centraleDB == null)
            {
                Centrale newCentrale = new Centrale() {
                    _id = Guid.NewGuid().ToString(),
                    userId = user._id,
                    reference = centraleRequest.reference,
                    type = centraleRequest.type,
                    capacite = centraleRequest.capacite,
                    dateCreation = DateTime.Now
                };

                Stock newStock = new Stock()
                {
                    _id = Guid.NewGuid().ToString(),
                    idCentrale = newCentrale._id,
                    quantite = 0,
                    dateCreation = newCentrale.dateCreation
                };

                _centrale.InsertOne(newCentrale);
                _stock.InsertOne(newStock);

                response.Content = new StringContent(newCentrale.reference + " a été créée.");
                response.StatusCode = HttpStatusCode.Created;
                return response;
            }
            else return Request.CreateErrorResponse(
                HttpStatusCode.BadRequest,
                new HttpException("La centrale " + centraleRequest.reference + " existe déjà pour le user " + user.login + "."));
        }

        private HttpResponseMessage refill(Usage refill)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            User user = getUserFromCookies();

            Centrale centraleDB = _centrale.AsQueryable().
            FirstOrDefault(c => c.userId == user._id && c.reference == refill.reference);
            if (centraleDB == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, new Exception("La centrale " + refill.reference + " n'a pas été trouvée."));

            Stock stockDB = _stock.AsQueryable().OrderByDescending(s => s.dateCreation).First(s => s.idCentrale == centraleDB._id);
            if (stockDB == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception("La base de données est corrompue. Le stock de la centrale " + centraleDB.reference + " est introuvable."));

            double quantiteDB = stockDB.quantite;
            double capaciteDB = centraleDB.capacite;
            double rechargeAcceptable = capaciteDB - quantiteDB;

            if ((quantiteDB + refill.quantite) > capaciteDB) return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, new Exception("La centrale n'accepte qu'une recharge de " + rechargeAcceptable + "KW au maximum."));

            Stock newStock = new Stock()
            {
                _id = Guid.NewGuid().ToString(),
                idCentrale = centraleDB._id,
                quantite = quantiteDB + refill.quantite,
                dateCreation = DateTime.Now
            };

            _stock.InsertOne(newStock);

            //refresh de la base pour actualiser le response.content
            stockDB = refreshStock(centraleDB);

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("La centrale " + centraleDB.reference + " a été rechargée de " + refill.quantite + " KW.\n" + stock(centraleDB, stockDB));
            return response;
        }

        private HttpResponseMessage consume(Usage consumption)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            User user = getUserFromCookies();

            Centrale centraleDB = _centrale.AsQueryable().
            FirstOrDefault(c => c.userId == user._id && c.reference == consumption.reference);
            if (centraleDB == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, new Exception("La centrale " + consumption.reference + " n'a pas été trouvée."));

            Stock stockDB = _stock.AsQueryable().OrderByDescending(s => s.dateCreation).First(s => s.idCentrale == centraleDB._id);
            if (stockDB == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception("La base de données est corrompue. Le stock de la centrale " + centraleDB.reference + " est introuvable."));

            double quantiteDB = stockDB.quantite;
            double capaciteDB = centraleDB.capacite;
            double rechargeAcceptable = capaciteDB - quantiteDB;

            if (consumption.quantite > quantiteDB) return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, new Exception("La centrale n'a qu'un stock de " + quantiteDB + "KW."));

            Stock newStock = new Stock()
            {
                _id = Guid.NewGuid().ToString(),
                idCentrale = centraleDB._id,
                quantite = quantiteDB - consumption.quantite,
                dateCreation = DateTime.Now
            };

            _stock.InsertOne(newStock);

            //refresh de la base pour actualiser le response.content
            stockDB = refreshStock(centraleDB);

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("La centrale " + centraleDB.reference + " a été vidée de " + consumption.quantite + " KW.\n" + stock(centraleDB, stockDB));
            return response;
        }

        private Stock refreshStock(Centrale centraleDB)
        {
            _stock = new utils.MongoDB().getCollection<Stock>("Stock");
            return _stock.AsQueryable().OrderByDescending(s => s.dateCreation).First(s => s.idCentrale == centraleDB._id);
        }

        private string stock(Centrale centraleDB, Stock stockDB)
        {
            string reference = centraleDB.reference;
            double stock = stockDB.quantite;
            double capacite = centraleDB.capacite;
            double pourcentageOccupation = 100 * stock / capacite;

            return "Stock de la centrale " + reference + " : \n\t" +
                    stock + " KW / " + capacite + " KW (" + pourcentageOccupation + " % utilisés)";
        }

        bool isSessionExpired()
        {
            if (Request == null || Request.Headers.GetCookies().Count == 0) return true;
            DateTime timeout;
            DateTime.TryParse(Request.Headers.GetCookies()[0]["timeout"].Value, out timeout);
            return timeout < DateTime.Now;
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
