﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayerCore.Model
{
    public partial class FINANCIAL_MATURITY
    {
        public FINANCIAL_MATURITY()
        {
            FINANCIAL_GROUPS = new HashSet<FINANCIAL_GROUPS>();
        }

        [Key]
        public int MaturityId { get; set; }
        [Required]
        [StringLength(255)]
        public string MaturityLevel { get; set; }
        [StringLength(50)]
        public string Acronym { get; set; }

        [InverseProperty("Maturity")]
        public virtual ICollection<FINANCIAL_GROUPS> FINANCIAL_GROUPS { get; set; }
    }
}