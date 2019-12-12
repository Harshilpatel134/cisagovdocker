//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Models
{
    public interface IExternalDocument
    {
        [Required]
        string Name { get; set; }
        [Required]
        string ShortName { get; set; }
        byte[] Data { get; set; }
        double? FileSize { get; set; }
        string FileName { get; set; }
    }
}

