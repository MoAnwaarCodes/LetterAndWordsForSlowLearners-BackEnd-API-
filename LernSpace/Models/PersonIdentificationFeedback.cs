//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LernSpace.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PersonIdentificationFeedback
    {
        public int id { get; set; }
        public int patientId { get; set; }
        public int personTestCollectionId { get; set; }
        public int personId { get; set; }
        public bool feedback { get; set; }
    
        public virtual Patient Patient { get; set; }
        public virtual Person Person { get; set; }
        public virtual PersonTestCollection PersonTestCollection { get; set; }
    }
}
