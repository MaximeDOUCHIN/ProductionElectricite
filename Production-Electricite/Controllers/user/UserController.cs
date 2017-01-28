using MongoDB.Driver;
using Production_Electricite.Controllers.utils;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Production_Electricite.Models;

namespace Production_Electricite.Controllers
{
    public class UserController : ApiController
    {
        protected IMongoCollection<User> _collection;

        [HttpPost]
        [Route("signin")]
        public HttpResponseMessage SignIn(User user)
        {
            if (ModelState.IsValid)
            {
                _collection = new Connexion().getCollection<User>("User");
                return signin(user);
            }
            else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [HttpPost]
        [Route("login")]
        public async Task<HttpResponseMessage> Login(User user)
        {
            if (ModelState.IsValid)
            {
                _collection = new Connexion().getCollection<User>("User");
                return await login(user);
            }
            else  return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        async Task<HttpResponseMessage> login(User user)
        {
            LoginInfos loginInfos = new LoginInfos();
            HttpResponseMessage response = new HttpResponseMessage();
            HttpResponseMessage unauthorized = Request.CreateErrorResponse(HttpStatusCode.Forbidden, new UnauthorizedAccessException());

            if (loginInfos.userExists(user))
            {
                User userDB = loginInfos.getUserFromRequest(user);

                string passwordDB = userDB.password;
                int nbTentativesDB = userDB.nbTentatives;
                DateTime timeout = new DateTime();
                DateTime.TryParse(Request.Headers.GetCookies()[0]["timeout"].Value, out timeout);

                if (nbTentativesDB >= 5)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Nombre de tentatives > 5. Réinitialisez le mot de passe."));
                }

                if (!loginInfos.isGoodPassword(user.password, userDB.password))
                {
                    User userUpdate = userDB; 
                    userUpdate.nbTentatives++;

                    var filter = Builders<User>.Filter.Eq(u => u.login, userDB.login);
                    var result = await _collection.ReplaceOneAsync(filter, userUpdate);

                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Mot de passe erroné, Nombre de tentatives restantes : " + (5 - nbTentativesDB).ToString() + "."));
                }

                response.Headers.AddCookies(new CookieHeaderValue[] {
                        createCookie("timeout", DateTime.Now.AddHours(4).ToString()),
                        createCookie("login", user.login)
                    });

                response.Content = new StringContent(user.login + " connecté avec succes.");
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            return unauthorized;
        }

        HttpResponseMessage signin(User user)
        {
            LoginInfos loginInfos = new LoginInfos();
            HttpResponseMessage response = new HttpResponseMessage();

            if (loginInfos.userExists(user))
            {
                response.Content = new StringContent("Le user " + user.login + " est déjà connu dans la base de données.");
                response.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                response.Content = new StringContent(user.login + " a été créé.");
                response.StatusCode = HttpStatusCode.Created;
                user._id = Guid.NewGuid().ToString();
                user.nbTentatives = 0;
                user.password = loginInfos.encrypt(user.password);
                _collection.InsertOne(user);
            }
            return response;
        }

        CookieHeaderValue createCookie(string name, string value)
        {
            CookieHeaderValue cookie = new CookieHeaderValue(name, value);
            cookie.Expires = DateTimeOffset.Now.AddDays(1);
            cookie.Domain = Request.RequestUri.Host;
            cookie.Path = "/";

            return cookie;
        }
    }
}
