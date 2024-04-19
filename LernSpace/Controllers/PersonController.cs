using LernSpace.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Http;

namespace LernSpace.Controllers
{
    public class PersonController : ApiController
    {
        SlowLearnerEntities db = new SlowLearnerEntities();

        [HttpPost]
        public HttpResponseMessage UploadPersonData()
        {
            var request = HttpContext.Current.Request;
            var name = request["name"];
            var gender = request["gender"];
            var age = int.Parse(request["age"]);
            var relation = request["relation"];
            var nameAudio = request.Files["audio"];
            var caregiverId = int.Parse(request["addBy"]);
            var personPics = request.Files.GetMultiple("personPics");

            Person p = new Person();
            p.name = name;
            p.age = age;
            p.gender = gender;
            p.relation = relation;
            p.addBy = caregiverId;

            if (nameAudio != null && nameAudio.ContentLength > 0)
            {
                string audioFileName = $"{p.id}.{nameAudio.FileName.Split('.')[1]}";
                nameAudio.SaveAs(HttpContext.Current.Server.MapPath("~/Media/persons/audios/" + audioFileName));
                p.audioPath = "/Media/persons/audios/" + audioFileName;
            }

            db.Person.Add(p);
            db.SaveChanges();


            if (personPics != null && personPics.Count > 0)
            {
                List<PersonPicture> pictureList = new List<PersonPicture>();
                foreach (var profilePic in personPics)
                {
                    PersonPicture picture = new PersonPicture();
                    picture.personid = p.id;

                    string username = profilePic.FileName.Split('.')[0];




                    string FileName = $"{p.id}.{profilePic.FileName.Split('.')[1]}";
                    profilePic.SaveAs(HttpContext.Current.Server.MapPath("~/Media/persons/Images/" + FileName));
                    picture.imgPath = "/Media/persons/Images/" + FileName;
                    pictureList.Add(picture);
                }
                db.PersonPicture.AddRange(pictureList);
                db.SaveChanges();
            }
            return Request.CreateResponse(HttpStatusCode.OK, "inserted");
        }
        [HttpGet]
        public HttpResponseMessage Getpersons(int CaregiverId)
        {
            var data = db.Person.Where(e => e.addBy == CaregiverId).Select(person => new
            {
                person.id,

                person.name

            }).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, data);
        }
        [HttpPost]

        public HttpResponseMessage Addpractice(PersonPracticeInfo info)
        {
            db.PersonPractice.Add(info.PersonPractice);
            db.SaveChanges();

            foreach (var item in info.Persons)
            {
                item.personPractice = info.PersonPractice.id;


            }
            db.PersonPracticCollection.AddRange(info.Persons);
            db.SaveChanges();
            return Request.CreateErrorResponse(HttpStatusCode.OK, "data save");
        }

        [HttpGet]
        public HttpResponseMessage GetPersonpracticesWithDetail(int uid)
        {
            var data = db.PersonPractice.Where(e => e.createdBy == uid)
                .Join(db.PersonPracticCollection, personprac => personprac.id, personPracColl => personPracColl.personPractice, (personPrac, personPracColl) => new { personPrac, personPracColl })
                .Join(db.Person, personpraccoll => personpraccoll.personPracColl.personId, person => person.id, (PersonPracColl, person) => new { PersonPracColl, person })
                .Join(db.PersonPicture, person => person.person.id, personpics => personpics.personid, (PersonPracColl, personpics) => new
                {
                    PInfo = PersonPracColl.PersonPracColl.personPrac,
                    personpics,
                    PersonDetail = PersonPracColl.person,
                    //PersonPracColl.person.name,
                    //PersonPracColl.person.audioPath,
                    //PersonPracColl.person.id,
                    //PersonPracColl.person.relation,
                    //personpics.imgPath,

                }).GroupBy(e => e.PInfo.title)
                    .Select(group => new
                    {
                        Id = group.Key,
                        PersonDetails = group.Select(item => new
                        {
                            item.PersonDetail,
                            item.personpics
                            //item.name,
                            //item.audioPath,
                            //item.relation,
                            //item.imgPath,
                        }),

                    });
            return Request.CreateResponse(HttpStatusCode.OK, data);
            /* var data = db.PersonPractice
             .Where(e => e.createdBy == uid)
             .Join(
                 db.PersonPracticCollection,
                 personprac => personprac.id,
                 personPracColl => personPracColl.personPractice,
                 (personPrac, personPracColl) => new { personPrac, personPracColl }
             )
             .Join(
                 db.Person,
                 personpraccoll => personpraccoll.personPracColl.personId,
                 person => person.id,
                 (PersonPracColl, person) => new { PersonPracColl, person }
             )
             .GroupJoin(
                 db.PersonPicture,
                 person => person.person.id,
                 personpics => personpics.personid,
                 (person, personpics) => new { person, personpics }
             )
             .SelectMany(
                 joined => joined.personpics.DefaultIfEmpty(),
                 (joined, personpic) => new
                 {
                     joined.person.PersonPracColl.personPracColl.Person.name,
                     joined.person.PersonPracColl.personPracColl.Person.audioPath,
                     joined.person.PersonPracColl.personPracColl.Person.id,
                     joined.person.PersonPracColl.personPracColl.Person.relation,
                     ImgPath = personpic == null ? null : personpic.imgPath,
                 }
             )
             .GroupBy(e => e.id)
             .Select(group => new
             {
                 Id = group.Key,
                 PersonDetails = group.Select(item => new
                 {
                     item.name,
                     item.audioPath,
                     item.relation,
                     item.ImgPath,
                 }),
             });*/




        }

        [HttpGet]
        public HttpResponseMessage GetPersonpractice(int uid)
        {
            var data = db.PersonPractice.Where(e => e.createdBy == uid).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, data);

            //    .Join(db.PersonPracticCollection, personprac => personprac.id, personPracColl => personPracColl.personPractice, (personPrac, personPracColl) => new { personPrac, personPracColl })
            //    .Join(db.Person, personpraccoll => personpraccoll.personPracColl.personId, person => person.id, (PersonPracColl, person) => new { PersonPracColl, person })
            //    .Join(db.PersonPicture, person => person.person.id, personpics => personpics.personid, (PersonPracColl, personpics) => new
            //    {
            //        PersonPracColl.person.name,
            //        PersonPracColl.person.audioPath,
            //        PersonPracColl.person.id,
            //        PersonPracColl.person.relation,
            //        personpics.imgPath,

            //    }).GroupBy(e => e.id)
            //        .Select(group => new
            //        {
            //            Id = group.Key,
            //            PersonDetails = group.Select(item => new
            //            {
            //                item.name,
            //                item.audioPath,
            //                item.relation,
            //                item.imgPath,
            //            }),

            //        });
            //return Request.CreateResponse(HttpStatusCode.OK, data);
            /* var data = db.PersonPractice
             .Where(e => e.createdBy == uid)
             .Join(
                 db.PersonPracticCollection,
                 personprac => personprac.id,
                 personPracColl => personPracColl.personPractice,
                 (personPrac, personPracColl) => new { personPrac, personPracColl }
             )
             .Join(
                 db.Person,
                 personpraccoll => personpraccoll.personPracColl.personId,
                 person => person.id,
                 (PersonPracColl, person) => new { PersonPracColl, person }
             )
             .GroupJoin(
                 db.PersonPicture,
                 person => person.person.id,
                 personpics => personpics.personid,
                 (person, personpics) => new { person, personpics }
             )
             .SelectMany(
                 joined => joined.personpics.DefaultIfEmpty(),
                 (joined, personpic) => new
                 {
                     joined.person.PersonPracColl.personPracColl.Person.name,
                     joined.person.PersonPracColl.personPracColl.Person.audioPath,
                     joined.person.PersonPracColl.personPracColl.Person.id,
                     joined.person.PersonPracColl.personPracColl.Person.relation,
                     ImgPath = personpic == null ? null : personpic.imgPath,
                 }
             )
             .GroupBy(e => e.id)
             .Select(group => new
             {
                 Id = group.Key,
                 PersonDetails = group.Select(item => new
                 {
                     item.name,
                     item.audioPath,
                     item.relation,
                     item.ImgPath,
                 }),
             });*/




        }
        [HttpGet]
        public HttpResponseMessage GetPersonPracticeDetail(int practiceId)
        {
            var data = db.PersonPractice.Where(e => e.id == practiceId)
                .Join(db.PersonPracticCollection, personprac => personprac.id, personPracColl => personPracColl.personPractice, (personPrac, personPracColl) => new { personPrac, personPracColl })
                .Join(db.Person, personpraccoll => personpraccoll.personPracColl.personId, person => person.id, (PersonPracColl, person) => new { PersonPracColl, person })
                .Join(db.PersonPicture, person => person.person.id, personpics => personpics.personid, (PersonPracColl, personpics) => new
                {
                    PersonPracColl.person.name,
                    PersonPracColl.person.audioPath,
                    PersonPracColl.person.id,

                    PersonPracColl.person.relation,
                    personpics.imgPath,

                }).Distinct();
            //.GroupBy(e => e.id)
            //    .Select(group => new
            //    {
            //        Id = group.Key,
            //        PersonDetails = group.Select(item => new
            //        {
            //            item.name,
            //            item.audioPath,
            //            item.relation,
            //            item.imgPath,
            //        }),

            //    });
            return Request.CreateResponse(HttpStatusCode.OK, data);
            /* var data = db.PersonPractice
             .Where(e => e.createdBy == uid)
             .Join(
                 db.PersonPracticCollection,
                 personprac => personprac.id,
                 personPracColl => personPracColl.personPractice,
                 (personPrac, personPracColl) => new { personPrac, personPracColl }
             )
             .Join(
                 db.Person,
                 personpraccoll => personpraccoll.personPracColl.personId,
                 person => person.id,
                 (PersonPracColl, person) => new { PersonPracColl, person }
             )
             .GroupJoin(
                 db.PersonPicture,
                 person => person.person.id,
                 personpics => personpics.personid,
                 (person, personpics) => new { person, personpics }
             )
             .SelectMany(
                 joined => joined.personpics.DefaultIfEmpty(),
                 (joined, personpic) => new
                 {
                     joined.person.PersonPracColl.personPracColl.Person.name,
                     joined.person.PersonPracColl.personPracColl.Person.audioPath,
                     joined.person.PersonPracColl.personPracColl.Person.id,
                     joined.person.PersonPracColl.personPracColl.Person.relation,
                     ImgPath = personpic == null ? null : personpic.imgPath,
                 }
             )
             .GroupBy(e => e.id)
             .Select(group => new
             {
                 Id = group.Key,
                 PersonDetails = group.Select(item => new
                 {
                     item.name,
                     item.audioPath,
                     item.relation,
                     item.ImgPath,
                 }),
             });*/




        }
        [HttpPost]
        public HttpResponseMessage AddPersonTest(PersonTestInfo info)
        {
            db.PersonTest.Add(info.Test);
            db.SaveChanges();
            foreach (var item in info.Persons)
            {
                item.personTestId = info.Test.id;
            }
            db.PersonTestCollection.AddRange(info.Persons);

            db.SaveChanges();
            return Request.CreateErrorResponse(HttpStatusCode.OK, "test Added");
        }
    }
}
