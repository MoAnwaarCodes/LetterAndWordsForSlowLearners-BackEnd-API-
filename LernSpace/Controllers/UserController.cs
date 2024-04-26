using LernSpace.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Http;

namespace LernSpace.Controllers
{
    public class UserController : ApiController
    {
        SlowLearnerEntities db = new SlowLearnerEntities();
        [HttpGet]
        public HttpResponseMessage deleteAppdata(int appid)
        {
            var data = db.Appointment.Where(e => e.id == appid).FirstOrDefault();
            db.Appointment.Remove(data);
            db.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK, "deleted");
        }
        [HttpPost]
        public HttpResponseMessage SignUp()
        {
            var request = HttpContext.Current.Request;
            var name = request["name"];
            var type = request["type"];
            var username = request["username"];
            var password = request["password"];
            var profilePic = request.Files["profilePic"];

            var validation = db.User.Where(e => e.username == username).ToList();
            if (validation.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, "username Already exist use another username");
            }
            string fileName = username.Split('.')[0] + "." + profilePic.FileName.Split('.')[1];
            profilePic.SaveAs(HttpContext.Current.Server.MapPath("~/Media/UsersImages/" + fileName));
            User user = new User();
            user.username = username;
            user.password = password;
            user.type = type;
            user.name = name;
            user.profPicPath = "/Media/UsersImages/" + fileName;
            db.User.Add(user);
            db.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK, "registerd");
        }


        [HttpGet]
        public HttpResponseMessage SignIn(string username, string password)
        {
            var data = db.User.Where(e => e.username == username && e.password == password).Select(user => new { user.uid, user.type, user.profPicPath, user.name }).FirstOrDefault();
            if (data == null)
            {
                var pdata = db.Patient.Where(e => e.userName == username && e.password == password).Select(user => new { user.pid, user.firstTime, user.profPicPath, user.name }).FirstOrDefault();
                if (pdata != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, pdata);
                }
                return Request.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, "invalid Username Or Password");
            }
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }
        [HttpGet]
        public HttpResponseMessage updateAppointmentFeedback(int appointid, string feedback)
        {
            var data = db.Appointment.Where(e => e.id == appointid).FirstOrDefault();
            data.feedback = feedback;
            db.SaveChanges();
            var newData = new { data.id, data.patientId, data.feedback };
            return Request.CreateResponse(HttpStatusCode.OK, newData);
        }
        [HttpGet]
        public HttpResponseMessage GetAppointments(int Did, DateTime date)
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;

            // Truncate time part from the date parameter
            DateTime startDate = date.Date;
            DateTime endDate = startDate.AddDays(1); // Assuming you want appointments for the entire day

            var data = db.Appointment
                          .Where(e => e.userId == Did && e.nextAppointDate >= startDate && e.nextAppointDate < endDate).
                          Join(db.Patient, app => app.patientId, pat => pat.pid, (app, pat) => new
                          {
                              app.id,
                              app.patientId,

                              app.feedback,
                              app.nextAppointDate,
                              pat.profPicPath,
                              pat.stage,
                              pat.name,

                              pat.age
                          })
                          /* Select(appoint => new {
                               appoint.id,
                               appoint.patientId,
                               appoint.pracId,
                               appoint.testId,
                               appoint.userId,
                               appoint.feedback,
                               appoint.appointmentDate,
                               appoint.nextAppointDate
                           })*/
                          .ToList();

            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpPost]
        public HttpResponseMessage AddAppointment(PatientAppointmet patientAppointmet)
        {
            try
            {
                db.Appointment.Add(patientAppointmet.Appointment);
                db.SaveChanges();
                if (patientAppointmet.AppointmentPractics != null)
                {
                    foreach (var item in patientAppointmet.AppointmentPractics)
                    {
                        item.appointmentId = patientAppointmet.Appointment.id;
                    }
                    db.AppointmentPractic.AddRange(patientAppointmet.AppointmentPractics);
                }
                if (patientAppointmet.AppointmentTests != null)
                {
                    foreach (var item in patientAppointmet.AppointmentTests)
                    {
                        item.appointmentId = patientAppointmet.Appointment.id;
                    }
                    db.AppointmentTest.AddRange(patientAppointmet.AppointmentTests);
                }


                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Appointment Add SuccessFully");
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message); }
        }
        [HttpGet]
        public HttpResponseMessage showSpacificAppointmentData(int AppointmentId, int pid)
        {
            var TestData = db.Appointment.Where(e => e.id == AppointmentId && e.patientId == pid)
                .Join(db.AppointmentTest, appoint => appoint.id, AppointmentTest => AppointmentTest.appointmentId, (appoint, appointmentTest) => new { appoint, appointmentTest })
                .Join(db.TestCollection, appointtest => appointtest.appointmentTest.testId, testcollection => testcollection.testId, (appoint, testcollection) => new { appoint, testcollection }).
                Join(db.PatientTestCollectionFeedback, ttc => ttc.testcollection.id, ptcf => ptcf.id, (appointTestCollection, pTestColletFedback) => new { appointTestCollection, pTestColletFedback })
                .Join(db.Collection, ptcf => ptcf.pTestColletFedback.collectionId, collect => collect.id, (all, collect) => new
                {
                    collect.eText,
                    collect.uText,
                    collect.type,
                    all.pTestColletFedback.feedback,


                });
            var PracticeData = db.Appointment.Where(e => e.id == AppointmentId && e.patientId == pid)
                .Join(db.AppointmentPractic, appoint => appoint.id, appoimtPrac => appoimtPrac.appointmentId, (appoint, appointmentPractic) => new { appoint, appointmentPractic })
                .Join(db.PracticeCollection, AppointmentPractic => AppointmentPractic.appointmentPractic.practiceId, practicecollection => practicecollection.pracId, (appoint, pracCollection) => new { appoint, pracCollection })


                .Join(db.Collection, ptcf => ptcf.pracCollection.collectId, collect => collect.id, (all, collect) => new
                {
                    collect.eText,
                    collect.uText,
                    collect.type,
                    all.appoint.appoint.userId,
                }).ToList();
            var uid = PracticeData[0].userId;
            var result = new { uid, PracticeData, TestData };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        [HttpPost]
        public HttpResponseMessage CaregiverRegisterPatient()
        {

            var request = HttpContext.Current.Request;
            var username = request["username"];
            var validation = db.Patient.Where(e => e.userName == username);

            if (validation == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Username Already exist");
            }

            var name = request["name"];
            var age = int.Parse(request["age"]);
            var gender = request["gender"];


            var password = request["password"];
            var profpic = request.Files["profpic"];
            var caregiverid = int.Parse(request["caregiverid"]);
            Patient patient = new Patient();

            patient.age = age;
            patient.gender = gender;
            patient.name = name;
            patient.userName = username;
            patient.stage = 1;
            patient.firstTime = true;
            patient.password = password;



            string fileName = patient.userName.Split('.')[0] + "." + profpic.FileName.Split('.')[1];
            profpic.SaveAs(HttpContext.Current.Server.MapPath("~/Media/PatientsImages/" + fileName));
            patient.profPicPath = "/Media/PatientsImages/" + fileName;
            db.Patient.Add(patient);
            db.SaveChanges();
            UserPatient CaregivePatient = new UserPatient();
            CaregivePatient.userId = caregiverid;
            CaregivePatient.patientId = patient.pid;
            db.UserPatient.Add(CaregivePatient);
            db.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK, "registerd");
        }

        [HttpGet]
        public HttpResponseMessage GetAllAppointmentsDates(int pid)
        {
            try
            {
                var data = db.Appointment.Where(e => e.patientId == pid).Select(appointdata => new
                {
                    appointdata.id,
                    appointdata.appointmentDate
                });
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetPatientId(int cid)
        {
            try
            {
                var data = db.User.Where(e => e.uid == cid)
                    .Join(db.UserPatient, user => user.uid, UserPatient => UserPatient.userId, (user, UserPatient) => new { user, UserPatient })
                    .Join(db.Appointment, user => user.UserPatient.patientId, Appointment => Appointment.patientId, (app, pat) => new
                    {
                        pat.id,
                        pat.patientId,

                        pat.feedback,
                        pat.nextAppointDate,

                    }).FirstOrDefault();
                /* {
                     UserPatient.patientId
                 }).FirstOrDefault(); */

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetAllpatiets(int Did)
        {
            try
            {
                var data = db.User.Where(e => e.uid == Did)
                    .Join(db.UserPatient, user => user.uid, userpatient => userpatient.userId, (user, userpatient) => new { user, userpatient })
                    .Join(db.Patient, UserPatient => UserPatient.userpatient.patientId, patient => patient.pid, (userPatient, patient) => new {
                        patient.pid,
                        patient.name,
                        patient.age,
                        patient.profPicPath

                    }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }

        }
    }
}
