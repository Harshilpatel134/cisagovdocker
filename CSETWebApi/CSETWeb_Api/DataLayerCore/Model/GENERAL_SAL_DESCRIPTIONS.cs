﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayerCore.Model
{
    public partial class GENERAL_SAL_DESCRIPTIONS
    {
        public GENERAL_SAL_DESCRIPTIONS()
        {
            GEN_SAL_WEIGHTS = new HashSet<GEN_SAL_WEIGHTS>();
        }

        [Key]
        [StringLength(50)]
        public string Sal_Name { get; set; }
        [Required]
        [StringLength(1024)]
        public string Sal_Description { get; set; }
        public int Sal_Order { get; set; }
        public int? min { get; set; }
        public int? max { get; set; }
        public int? step { get; set; }
        [StringLength(50)]
        public string prefix { get; set; }
        [StringLength(50)]
        public string postfix { get; set; }

        [InverseProperty("Sal_NameNavigation")]
        public virtual ICollection<GEN_SAL_WEIGHTS> GEN_SAL_WEIGHTS { get; set; }
    }
}