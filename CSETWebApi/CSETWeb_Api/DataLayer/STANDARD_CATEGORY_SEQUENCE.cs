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
    
    public partial class STANDARD_CATEGORY_SEQUENCE
    {
        public string Standard_Category { get; set; }
        public string Set_Name { get; set; }
        public int Standard_Category_Sequence1 { get; set; }
    
        public virtual SET SET { get; set; }
        public virtual STANDARD_CATEGORY STANDARD_CATEGORY1 { get; set; }
    }
}


