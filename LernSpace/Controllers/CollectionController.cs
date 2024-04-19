using LernSpace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Web.Http;

namespace LernSpace.Controllers
{
  
    public class CollectionController : ApiController
    {
        SlowLearnerEntities db = new SlowLearnerEntities();
        [HttpPost]
        public HttpResponseMessage AddColletion()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var Utext = request["utext"];
                var Etext = request["etext"];
                var validation = db.Collection.Where(e => e.eText == Etext || e.uText == Utext);
                if (validation.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Data already exist");
                }
                var Type = request["type"];
                var group = request["group"];
                var Picture = request.Files["picture"];
                var Audio = request.Files["audio"];
                string picfileName = Etext + "." + Picture.FileName.Split('.')[1];

                string audiofileName = Etext + "." + Audio.FileName.Split('.')[1];

                Collection c = new Collection();
                c.uText = Utext;
                c.eText = Etext;
                c.type = Type;
                c.C_group = group;

                if (Etext.Length == 1)
                {
                    Picture.SaveAs(HttpContext.Current.Server.MapPath("~/Media/Alphabets/Images/" + picfileName));

                    Audio.SaveAs(HttpContext.Current.Server.MapPath("~/Media/Alphabets/audios/" + audiofileName));

                    c.audioPath = "/Media/Alphabets/audios/" + audiofileName;
                    c.picPath = "/Media/Alphabets/images/" + picfileName;
                }
                else
                {
                    // Split the input into words
                    string[] words = Type.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (words.Length == 1)
                    {
                        Picture.SaveAs(HttpContext.Current.Server.MapPath("~/Media/Words/Images/" + picfileName));

                        Audio.SaveAs(HttpContext.Current.Server.MapPath("~/Media/Words/audios/" + audiofileName));

                        c.audioPath = "/Media/Words/audios/" + audiofileName;
                        c.picPath = "/Media/Words/images/" + picfileName;
                    }
                    else
                    {
                        Picture.SaveAs(HttpContext.Current.Server.MapPath("~/Media/Sentences/Images/" + picfileName));

                        Audio.SaveAs(HttpContext.Current.Server.MapPath("~/Media/Sentences/audios/" + audiofileName));

                        c.audioPath = "/Media/Sentences/audios/" + audiofileName;
                        c.picPath = "/Media/Sentences/images/" + picfileName;
                    }
                }



                db.Collection.Add(c);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "collection add successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetAllCollection()
        {
            var data = db.Collection.Select(c => new { c.id, c.picPath, c.eText, c.uText, c.type, c.C_group }).ToList();
            return Request.CreateResponse(data);
        }
        [HttpGet]
        public HttpResponseMessage GetCollectionImgPath(int Cid)
        {

            var data = db.Collection.Where(e => e.id == Cid).Select(c => new { c.picPath, c.eText, c.uText }).ToList();
            return Request.CreateResponse(data);
        }
    }
}
