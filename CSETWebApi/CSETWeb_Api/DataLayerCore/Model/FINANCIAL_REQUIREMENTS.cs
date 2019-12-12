﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayerCore.Model
{
    public partial class FINANCIAL_REQUIREMENTS
    {
        public int StmtNumber { get; set; }
        public int Requirement_Id { get; set; }
        public int? ID { get; set; }

        [ForeignKey("Requirement_Id")]
        [InverseProperty("FINANCIAL_REQUIREMENTS")]
        public virtual NEW_REQUIREMENT Requirement_ { get; set; }
        [ForeignKey("StmtNumber")]
        [InverseProperty("FINANCIAL_REQUIREMENTS")]
        public virtual FINANCIAL_DETAILS StmtNumberNavigation { get; set; }
    }
}