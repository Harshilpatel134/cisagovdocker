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
    
    public partial class IRP
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public IRP()
        {
            this.ASSESSMENT_IRP = new HashSet<ASSESSMENT_IRP>();
        }
    
        public int IRP_ID { get; set; }
        public Nullable<int> Item_Number { get; set; }
        public string Risk_1_Description { get; set; }
        public string Risk_2_Description { get; set; }
        public string Risk_3_Description { get; set; }
        public string Risk_4_Description { get; set; }
        public string Risk_5_Description { get; set; }
        public string Validation_Approach { get; set; }
        public int Header_Id { get; set; }
        public string Description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ASSESSMENT_IRP> ASSESSMENT_IRP { get; set; }
        public virtual IRP_HEADER IRP_HEADER { get; set; }
    }
}
