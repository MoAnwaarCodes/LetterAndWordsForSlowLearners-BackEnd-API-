using LernSpace.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Web.Http;
using System.Web.UI;

namespace LernSpace.Controllers
{
    
    public class PracticeController : ApiController
    {

        SlowLearnerEntities db = new SlowLearnerEntities();
        [HttpPost]
        /*Practic practic, [FromUri] int[] collection*/
        public HttpResponseMessage AddNewPractice([FromBody] PracticeInfo info)
        {
            try
            {
                db.Practice.Add(info.practice);
                db.SaveChanges();


                foreach (var item in info.collections)
                {
                    item.pracId = info.practice.id;


                }
                db.PracticeCollection.AddRange(info.collections);
                db.SaveChanges();
                return Request.CreateErrorResponse(HttpStatusCode.OK, "data save");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage getPracticeDetail(int pid)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var data = db.Practice.Where(e => e.id == pid)

                .Join(db.PracticeCollection, practic => practic.id, pracCollection => pracCollection.pracId, (practic, pracCollection) => new { practic, pracCollection })
                .Join(db.Collection, ppc => ppc.pracCollection.collectId, collect => collect.id, (ppc, collect) => new
                {

                    collect.id,
                    collect.uText,
                    collect.eText,
                    collect.type,
                    collect.picPath,
                    collect.C_group,
                    collect.audioPath,
                    ppc.practic.title,
                    pracId = ppc.practic.id


                });

            if (data.Any())
            {

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "data not found");
        }

        [HttpGet]
        public HttpResponseMessage getPractices(int Uid)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var data = db.Practice.Where(e => e.createBy == Uid).ToList();



            if (data.Any())
            {

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "data not found");
        }
    }
}
