﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayerCore.Model
{
    public partial class RECENT_FILES
    {
        [StringLength(512)]
        public string AssessmentName { get; set; }
        [Required]
        [StringLength(900)]
        public string Filename { get; set; }
        [Required]
        [StringLength(1024)]
        public string FilePath { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastOpenedTime { get; set; }
        public int RecentFileId { get; set; }
    }
}