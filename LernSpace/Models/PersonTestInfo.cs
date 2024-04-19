using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LernSpace.Models
{
    public class PersonTestInfo
    {
        public PersonTest Test { get; set; }
        public List<PersonTestCollection> Persons { get; set; }
    }
}