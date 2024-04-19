using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LernSpace.Models
{
    public class PatientAppointmet
    {
        public Appointment Appointment { get; set; }
        public List<AppointmentPractic> AppointmentPractics { get; set; }
        public List<AppointmentTest> AppointmentTests { get; set; }

    }
}