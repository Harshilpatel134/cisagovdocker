//////////////////////////////// 
// 
//   Copyright 2018 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class COMPONENT_STANDARD_QUESTIONS
    {
        public int Question_Id { get; set; }
        public int Requirement_id { get; set; }
        public string Component_Type { get; set; }
    
        public virtual NEW_QUESTION NEW_QUESTION { get; set; }
        public virtual NEW_REQUIREMENT NEW_REQUIREMENT { get; set; }
    }
}


