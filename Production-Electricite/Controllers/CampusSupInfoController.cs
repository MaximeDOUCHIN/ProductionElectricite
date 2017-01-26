using MongoDB.Driver;
using Production_Electricite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Configuration;
using System.Web;

namespace Production_Electricite.Controllers
{
    public class CampusSupInfoController : ApiController
    {

        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        IMongoCollection<CampusSupInfo> _collection;

        /// <summary>
        /// récupère une collection MongoDB
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IMongoCollection<CampusSupInfo> getCollection(string collection)
        {
            return _database.GetCollection<CampusSupInfo>(collection);
        }

        void connectToMongo()
        {
            _client = new MongoClient("mongodb://localhost:27017");
            _database = _client.GetDatabase("local");
        }

        bool CampusExists(CampusSupInfo campus)
        {

            var test = _collection.AsQueryable().FirstOrDefault(
                c => (
                    c.Nom == campus.Nom
                    && c.Adresse == campus.Adresse
                    && c.CodePostal == campus.CodePostal
                    && c.Ville == campus.Ville
                )
            );

            return test != null;
        }

        HttpResponseMessage conformiteCampus(CampusSupInfo campus)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            string content="";

            if (campus != null)
            {
                if (campus.Nom == null) { content += "Le nom (Nom) du campus doit être renseigné\n"; }
                if (campus.Adresse == null) { content += "L'adresse (Adresse) du campus doit être renseignée\n"; }
                if (campus.CodePostal == null) { content += "Le code postal (CodePostal) du campus doit être renseigné\n"; }
                if (campus.Ville == null) { content += "La ville (Ville) du campus doit être renseignée\n"; }

                if (CampusExists(campus))
                {
                    content = "le campus " + campus.Nom + " existe déjà dans la base de données.";
                }
            }
            else
            {
                content = "Le flux envoyé ne correspond pas à un campus";
            }
            if (content != "")
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent(content);
            }

            return response;
        }

        /// <summary>
        /// Insert un nouveau campus
        /// </summary>
        /// <param name="campus"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("insert")]
        public HttpResponseMessage Insert(CampusSupInfo campus)
        {
            connectToMongo();
            _collection = getCollection("CampusSupInfo");

            HttpResponseMessage response = conformiteCampus(campus);

            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                    campus._id = Guid.NewGuid().ToString();
                    _collection.InsertOne(campus);
                    response.StatusCode = HttpStatusCode.Created;
                    response.Content = new StringContent("le campus " + campus.Nom + " a été inséré.");
            }

            return response;
        }


        //[HttpGet]
        //[Route("api/v1/campussupinfo")]
        //public async Task<IEnumerable<CampusSupInfo>> GetAllCampusSupInfo()
        //{

        //   /*  MongoClient server = new MongoClient("mongodb://localhost:27017");
        //     IMongoDatabase dataBase = server.GetDatabase("local");
        //     IMongoCollection<CampusSupInfo> collection = dataBase.GetCollection<CampusSupInfo>("CampusSupInfo");
        //    var list = await collection.Find(toto => toto.Nom == "Campus2").ToListAsync();*/
        //    return listeCampusSupInfo;
        //}

        //[HttpPut]
        //[Route("api/v1/campussupinfo")]
        //public IHttpActionResult AddCampusSupInfo(CampusSupInfo newCampusSupInfo)
        //{
        //    var IdCampus = listeCampusSupInfo.Count() + 1;
        //    //newCampusSupInfo._id = IdCampus;
        //    listeCampusSupInfo.Add(newCampusSupInfo);

        //    return Ok(newCampusSupInfo);
        //}

        [HttpPut]
        [Route("api/v1/campussupinfo")]
        public HttpResponseMessage UpdateCampusSupInfo(CampusSupInfo campus)
        {
            //CampusSupInfo campusSupInfo = listeCampusSupInfo.FirstOrDefault(c => c._id == campus._id);

            //if (campusSupInfo == null)
            //    throw new HttpResponseException(HttpStatusCode.NotFound);
            //campusSupInfo.Nom = campus.Nom;
            //campusSupInfo.Ville = campus.Ville;
            //campusSupInfo.CodePostal = campus.CodePostal;
            //campusSupInfo.Adresse = campus.Adresse;

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("api/v1/campussupinfo/{idCampus}")]
        public HttpResponseMessage DeleteCampusSupInfo(int idCampus)
        {
            //CampusSupInfo campusSupInfo = listeCampusSupInfo.FirstOrDefault(c => c._id == idCampus);

            //if (campusSupInfo == null)
            //    throw new HttpResponseException(HttpStatusCode.NotFound);
            //listeCampusSupInfo.Remove(campusSupInfo);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
