//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSETWeb_Api.Models
{
    public class StandardsResponse
    {
        public List<StandardCategory> Categories;
        public int QuestionCount;
        public int RequirementCount;
    }


    /// <summary>
    /// 
    /// </summary>
    public class StandardCategory
    {
        public string CategoryName { get; set; }
        public List<Standard> Standards { get; set; }

        public StandardCategory()
        {
            Standards = new List<Standard>();
        }
    }


    /// <summary>
    /// This is the model for the Standard that is displayed on the 
    /// Cybersecurity Standard Selection screen.
    /// </summary>
    public class Standard
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public bool Selected { get; set; }
        public bool Recommended { get; set; }
    }
}

