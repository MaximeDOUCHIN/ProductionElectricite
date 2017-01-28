using MongoDB.Driver;
using Production_Electricite.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Production_Electricite.Controllers.utils;

namespace Production_Electricite.Controllers
{
    public class CampusSupInfoController : ApiController
    {
        protected IMongoCollection<CampusSupInfo> _collection;

        HttpResponseMessage insert(CampusSupInfo campus)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            if (_collection.AsQueryable().FirstOrDefault(
                c => (
                    c.Nom == campus.Nom
                    && c.Adresse == campus.Adresse
                    && c.CodePostal == campus.CodePostal
                    && c.Ville == campus.Ville
                )
            ) != null)
            {
                response.Content = new StringContent("Le campus " + campus.Nom + " existe déjà dans la base de données.");
                response.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                response.Content = new StringContent("Le campus " + campus.Nom + " a été inséré.");
                response.StatusCode = HttpStatusCode.Created;
                campus._id = Guid.NewGuid().ToString();
                _collection.InsertOne(campus);
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
            _collection = new utils.MongoDB().getCollection<CampusSupInfo>("CampusSupInfo");

            HttpResponseMessage response = new HttpResponseMessage();

            if (ModelState.IsValid)
            {
                return insert(campus);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
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
