using LernSpace.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.ModelBinding;

namespace LernSpace.Controllers
{
     public class PatientController : ApiController
    {

        SlowLearnerEntities db = new SlowLearnerEntities();
        [HttpGet]
        public HttpResponseMessage AssignedPractic(int Pid, string filter)
        {
            var data = db.Patient
             .Where(e => e.pid == Pid)
             .Join(db.Appointment, p => p.pid, ap => ap.patientId, (patien, appoint) => new { patien, appoint })
             .Join(db.AppointmentPractic, p => p.appoint.id, c => c.appointmentId, (appointment, appointPractic) => new { appointment, appointPractic })
             .Join(db.PracticeCollection, ap => ap.appointPractic.practiceId, pc => pc.pracId, (appoint, pracCollect) => new { appoint, pracCollect })
             .Join(db.Collection, prac => prac.pracCollect.collectId, c => c.id, (pracCollect, collect) => new
             {
                 AppointmentDate = pracCollect.appoint.appointPractic.Appointment.id,
                 practiceCollectionId = pracCollect.pracCollect.id,
                 CollectId = collect.id,
                 Path = collect.picPath,
                 Etext = collect.eText,
                 Utext = collect.uText,
                 Group = collect.C_group,
                 Type = collect.type,
                 collect.audioPath,
             })
             .OrderByDescending(e => e.AppointmentDate)
             .GroupBy(e => e.AppointmentDate)
             .Select(group => new
             {
                 Appointment = group.Key,
                 Collections = group.Select(e => new
                 {
                     e.practiceCollectionId,
                     e.CollectId,
                     e.Path,
                     e.Etext,
                     e.Utext,
                     e.Group,
                     e.Type,
                     e.audioPath,
                     e.AppointmentDate

                 }).ToList()
             });
            // .FirstOrDefault();
            if (filter == "all")
            {
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }


            return Request.CreateResponse(HttpStatusCode.OK, data.FirstOrDefault());
        }

        [HttpGet]
        public HttpResponseMessage AssignedTest(int Pid, string filter)
        {
            var data = db.Patient
             .Where(e => e.pid == Pid)
             .Join(db.Appointment, p => p.pid, ap => ap.patientId, (patien, appoint) => new { patien, appoint })
             .Join(db.AppointmentTest, p => p.appoint.id, c => c.appointmentId, (appointment, appointTest) => new { appointment, appointTest })
             .Join(db.TestCollection, ap => ap.appointTest.testId, tc => tc.testId, (app, testcollect) => new { app, testcollect })
             .Join(db.Collection, test => test.testcollect.collectId, c => c.id, (testCollect, collect) => new
             {
                 AppointmentDate = testCollect.app.appointment.appoint.id,
                 testCollectionID = testCollect.testcollect.id,
                 CollectId = collect.id,
                 Path = collect.picPath,
                 Etext = collect.eText,
                 Utext = collect.uText,
                 Group = collect.C_group,
                 Type = collect.type,
                 collectAudio = collect.audioPath,
                 Opt1 = testCollect.testcollect.op1,
                 Opt2 = testCollect.testcollect.op2,
                 Opt3 = testCollect.testcollect.op3,
                 Question = testCollect.testcollect.questionTitle,
                 Op1ImagePath = db.Collection.Where(c => c.id == testCollect.testcollect.op1).Select(c => c.picPath).FirstOrDefault(),
                 Op2ImagePath = db.Collection.Where(c => c.id == testCollect.testcollect.op2).Select(c => c.picPath).FirstOrDefault(),
                 Op3ImagePath = db.Collection.Where(c => c.id == testCollect.testcollect.op3).Select(c => c.picPath).FirstOrDefault(),
                 Op1Audio = db.Collection.Where(c => c.id == testCollect.testcollect.op1).Select(c => c.audioPath).FirstOrDefault(),
                 Op2Audio = db.Collection.Where(c => c.id == testCollect.testcollect.op2).Select(c => c.audioPath).FirstOrDefault(),
                 Op3Audio = db.Collection.Where(c => c.id == testCollect.testcollect.op3).Select(c => c.audioPath).FirstOrDefault(),

             })
             .OrderByDescending(e => e.AppointmentDate)
             .GroupBy(e => e.AppointmentDate)
             .Select(group => new
             {
                 AppointmentDate = group.Key,
                 Collections = group.Select(e => new
                 {
                     e.testCollectionID,
                     e.CollectId,
                     e.Path,
                     e.Etext,
                     e.Utext,
                     e.Group,
                     e.Type,
                     e.Opt1,
                     e.Opt2,
                     e.Opt3,
                     e.Op1ImagePath,
                     e.Op2ImagePath,
                     e.Op3ImagePath,
                     e.Question,
                     e.collectAudio,
                     e.Op1Audio,
                     e.Op2Audio,
                     e.Op3Audio,
                 }).ToList()
             });


            if (filter == "all")
            {
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }


            return Request.CreateResponse(HttpStatusCode.OK, data.FirstOrDefault());


        }
        [HttpGet]
        public HttpResponseMessage computeTestResult([FromBody] int[] SelectedAns, int testId, int pid)
        {


            var data = db.TestCollection.Where(e => e.testId == testId)
                .Join(db.Collection, tc => tc.collectId, coll => coll.id, (testColl, coll) => new
                {
                    testColl.id,
                    collectId = coll.id
                }).ToList();
            bool[] result = new bool[SelectedAns.Length];
            int count = 0;
            foreach (var item in data)
            {
                if (item.collectId == SelectedAns[count])
                {
                    result[count] = true;
                }
                count++;
            }
            List<PatientTestCollectionFeedback> patientTestCollectionFeedback = new List<PatientTestCollectionFeedback>();

            count = 0;
            foreach (var item in data)
            {
                PatientTestCollectionFeedback p = new PatientTestCollectionFeedback();
                p.patientId = pid;
                p.testCollectionId = item.id;
                p.collectionId = item.collectId;
                p.feedback = result[count];
                patientTestCollectionFeedback.Add(p);
                count++;
            }
            db.PatientTestCollectionFeedback.AddRange(patientTestCollectionFeedback);
            db.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK, patientTestCollectionFeedback);

        }
    }
}
