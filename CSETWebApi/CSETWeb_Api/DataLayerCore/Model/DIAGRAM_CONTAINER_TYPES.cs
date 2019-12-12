﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayerCore.Model
{
    public partial class DIAGRAM_CONTAINER_TYPES
    {
        public DIAGRAM_CONTAINER_TYPES()
        {
            DIAGRAM_CONTAINER = new HashSet<DIAGRAM_CONTAINER>();
        }

        [Key]
        [StringLength(50)]
        public string ContainerType { get; set; }

        [InverseProperty("ContainerTypeNavigation")]
        public virtual ICollection<DIAGRAM_CONTAINER> DIAGRAM_CONTAINER { get; set; }
    }
}