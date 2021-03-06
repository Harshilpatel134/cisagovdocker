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
    
    public partial class NCSF_CATEGORY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NCSF_CATEGORY()
        {
            this.NEW_REQUIREMENT = new HashSet<NEW_REQUIREMENT>();
        }
    
        public string NCSF_Function_Id { get; set; }
        public string NCSF_Category_Id { get; set; }
        public string NCSF_Category_Name { get; set; }
        public string NCSF_Category_Description { get; set; }
        public int NCSF_Cat_Id { get; set; }
        public int Question_Group_Heading_Id { get; set; }
    
        public virtual NCSF_FUNCTIONS NCSF_FUNCTIONS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NEW_REQUIREMENT> NEW_REQUIREMENT { get; set; }
    }
}


