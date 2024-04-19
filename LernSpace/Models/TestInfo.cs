using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LernSpace.Models
{
    public class TestInfo
    {
        public Test test { get; set; }
        public List<TestCollection> collectionsIds { get; set; }
    }
}