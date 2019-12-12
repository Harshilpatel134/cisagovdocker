﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayerCore.Model
{
    public partial class STANDARD_CATEGORY
    {
        public STANDARD_CATEGORY()
        {
            NEW_REQUIREMENT = new HashSet<NEW_REQUIREMENT>();
            STANDARD_CATEGORY_SEQUENCE = new HashSet<STANDARD_CATEGORY_SEQUENCE>();
        }

        [Key]
        [Column("Standard_Category")]
        [StringLength(250)]
        public string Standard_Category1 { get; set; }

        [InverseProperty("Standard_CategoryNavigation")]
        public virtual ICollection<NEW_REQUIREMENT> NEW_REQUIREMENT { get; set; }
        [InverseProperty("Standard_CategoryNavigation")]
        public virtual ICollection<STANDARD_CATEGORY_SEQUENCE> STANDARD_CATEGORY_SEQUENCE { get; set; }
    }
}