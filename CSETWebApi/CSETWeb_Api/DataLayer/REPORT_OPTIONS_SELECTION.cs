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
    
    public partial class REPORT_OPTIONS_SELECTION
    {
        public int Assessment_Id { get; set; }
        public int Report_Option_Id { get; set; }
        public bool Is_Selected { get; set; }
    
        public virtual ASSESSMENT ASSESSMENT { get; set; }
        public virtual REPORT_OPTIONS REPORT_OPTIONS { get; set; }
    }
}


