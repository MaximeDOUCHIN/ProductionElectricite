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
        public HttpResponseMessage SignIn(User userRequest)
        {
            if (ModelState.IsValid)
            {
                _collection = new utils.MongoDB().getCollection<User>("User");
                return signin(userRequest);
            }
            else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [HttpPost]
        [Route("login")]
        public async Task<HttpResponseMessage> Login(User userRequest)
        {
            if (ModelState.IsValid)
            {
                _collection = new utils.MongoDB().getCollection<User>("User");
                return await login(userRequest);
            }
            else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [HttpGet]
        [Route("logout")]
        public HttpResponseMessage Logout()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            User user = getUserFromCookies();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Utilisateur déconnecté.");

            if (user == null) return response;

            response.Content = new StringContent("Utilisateur " + user.login + " déconnecté.");
            response.Headers.AddCookies(new CookieHeaderValue[] {
                deleteCookie("login", user.login),
                deleteCookie("timeout", "")
            });

            return response;
        }

        async Task<HttpResponseMessage> login(User userRequest)
        {
            LoginInfos loginInfos = new LoginInfos();
            HttpResponseMessage response = new HttpResponseMessage();
            HttpResponseMessage unauthorized = Request.CreateErrorResponse(HttpStatusCode.Forbidden, new UnauthorizedAccessException());

            if (loginInfos.userExists(userRequest))
            {
                User userDB = loginInfos.getUserFromRequest(userRequest);

                string passwordDB = userDB.password;
                int nbTentativesDB = userDB.nbTentatives;
                var filter = Builders<User>.Filter.Eq(u => u.login, userDB.login);


                if (nbTentativesDB == 5)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Nombre de tentatives > 5. Réinitialisez le mot de passe."));
                }

                if (!loginInfos.isGoodPassword(userRequest.password, userDB.password))
                {
                    userDB.nbTentatives++;
                    await _collection.ReplaceOneAsync(filter, userDB);

                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Mot de passe erroné, Nombre de tentatives restantes : " + (4 - nbTentativesDB).ToString() + "."));
                }

                userDB.nbTentatives = 0;
                await _collection.ReplaceOneAsync(filter, userDB);

                response.Headers.AddCookies(new CookieHeaderValue[] {
                        createCookie("timeout", DateTime.Now.AddHours(4).ToString()),
                        createCookie("login", userRequest.login)
                    });

                response.Content = new StringContent(userRequest.login + " connecté avec succès.");
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            return unauthorized;
        }

        HttpResponseMessage signin(User userRequest)
        {
            LoginInfos loginInfos = new LoginInfos();
            HttpResponseMessage response = new HttpResponseMessage();

            if (loginInfos.userExists(userRequest))
            {
                response.Content = new StringContent("Le user " + userRequest.login + " est déjà connu dans la base de données.");
                response.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                response.Content = new StringContent("L'utilisateur " + userRequest.login + " a été créé.");
                response.StatusCode = HttpStatusCode.Created;
                userRequest._id = Guid.NewGuid().ToString();
                userRequest.nbTentatives = 0;
                userRequest.password = loginInfos.encrypt(userRequest.password);
                _collection.InsertOne(userRequest);
            }
            return response;
        }

        CookieHeaderValue createCookie(string name, string value)
        {
            CookieHeaderValue cookie = new CookieHeaderValue(name, value)
            {
                Expires = DateTimeOffset.Now.AddDays(1),
                Domain = Request.RequestUri.Host,
                Path = "/"
            };

            return cookie;
        }

        CookieHeaderValue deleteCookie(string name, string value)
        {
            CookieHeaderValue cookie = createCookie(name, value);
            cookie.Expires = DateTimeOffset.Now.AddDays(-1);

            return cookie;
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
