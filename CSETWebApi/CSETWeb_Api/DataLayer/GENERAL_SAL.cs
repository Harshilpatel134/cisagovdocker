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
    
    public partial class GENERAL_SAL
    {
        public int Assessment_Id { get; set; }
        public string Sal_Name { get; set; }
        public int Slider_Value { get; set; }
    
        public virtual ASSESSMENT ASSESSMENT { get; set; }
        public virtual GEN_SAL_NAMES GEN_SAL_NAMES { get; set; }
    }
}


