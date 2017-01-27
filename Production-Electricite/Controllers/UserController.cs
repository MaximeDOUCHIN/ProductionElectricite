using MongoDB.Driver;
using Production_Electricite.Controllers.utils;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Security.Cryptography;
using System;
using System.Web;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

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
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

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
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        async Task<HttpResponseMessage> login(User user)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            HttpResponseMessage unauthorized = Request.CreateErrorResponse(HttpStatusCode.Forbidden, new UnauthorizedAccessException());

            if (userExists(user))
            {
                User userDB = _collection.AsQueryable().FirstOrDefault(u => u.login == user.login);

                string passwordDB = userDB.password;
                int nbTentativesDB = userDB.nbTentatives;
                DateTime timeout = new DateTime();
                DateTime.TryParse(Request.Headers.GetCookies()[0]["timeout"].Value, out timeout);

                if (nbTentativesDB >= 5)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Nombre de tentatives > 5. Réinitialisez le mot de passe."));
                }

                if (!isGoodPassword(user.password, userDB.password))
                {
                    User userUpdate = userDB; 
                    userUpdate.nbTentatives++;

                    var filter = Builders<User>.Filter.Eq(u => u.login, userDB.login);
                    var result = await _collection.ReplaceOneAsync(filter, userUpdate);

                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Mot de passe erroné, Nombre de tentatives restantes : " + (5 - nbTentativesDB).ToString() + "."));
                }

                response.Headers.AddCookies(new CookieHeaderValue[] {
                        createCookie("timeout", DateTime.Now.AddSeconds(5).ToString()),
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
            HttpResponseMessage response = new HttpResponseMessage();

            if (userExists(user))
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
                user.password = encrypt(user.password);
                _collection.InsertOne(user);
            }
            return response;
        }

        private bool userExists(User user)
        {
            return _collection.AsQueryable().FirstOrDefault(u => u.login == user.login) != null;
        }

        private CookieHeaderValue createCookie(string name, string value)
        {
            CookieHeaderValue cookie = new CookieHeaderValue(name, value);
            cookie.Expires = DateTimeOffset.Now.AddDays(1);
            cookie.Domain = Request.RequestUri.Host;
            cookie.Path = "/";

            return cookie;
        }

        private string encrypt(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }

        private bool isGoodPassword(string passwordRequest, string passwordDB)
        {
            byte[] hashBytes = Convert.FromBase64String(passwordDB);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(passwordRequest, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }
    }
}