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
    
    public partial class SET_FILES
    {
        public string SetName { get; set; }
        public int Gen_File_Id { get; set; }
        public string Comment { get; set; }
    
        public virtual GEN_FILE GEN_FILE { get; set; }
        public virtual SET SETS { get; set; }
    }
}
