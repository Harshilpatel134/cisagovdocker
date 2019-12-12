//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWeb_Api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSETWeb_Api.BusinessManagers.Analysis
{

    public class GetCombinedOveralls
    {
        public string StatType { get; set; }
        public int? Total { get; set; }
        public int? Y { get; set; }
        public int? N { get; set; }
        public int? NA { get; set; }
        public int? A { get; set; }
        public int? U { get; set; }
        public int? YCount { get; set; }
        public int? NCount { get; set; }
        public int? NACount { get; set; }
        public int? ACount { get; set; }
        public int? UCount { get; set; }
        public double Value { get; set; }
        public int? TotalNoNA { get; set; }
    }
    public class usp_getRankedCategories
    {
        public String Question_Group_Heading { get; set; }
        public int? qc { get; set; }
        public int? cr { get; set; }
        public int? Total { get; set; }
        public int? nuCount { get; set; }
        public int? Actualcr { get; set; }
        public decimal? prc { get; set; }
        public decimal? Percent { get; set; }
    }

    public class FirstPage
    {
        public ChartData OverallBars { get; set; }
        public ChartData RedBars { get; set; }
        public ChartData StandardsSummaryPie { get; set; }
        public ChartData ComponentSummaryPie { get; set; }
    }


    public class ChartData
    {
        //this is hokey sorry
        //but check this list if it is not empty then use this
        //list otherwise user the rest of the data
        public List<ChartData> dataSets { get; set; }


        public string label { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public string borderWidth { get; set; }
        public List<double> data { get; set; }
        public List<String> Labels { get; set; }
        public int ComponentCount { get; set; }
        public List<DataRows> DataRows { get; set; }
        public List<DataRowsPie> DataRowsPie { get; set; }
        public List<string> Colors { get; set; }

        public ChartData()
        {
            data = new List<double>();
            Labels = new List<string>();
            dataSets = new List<ChartData>();
            DataRows = new List<DataRows>();
        }
    }

    public class DataRows
    {
        public string title { get; set; }
        public Decimal? rank { get; set; }
        public int? failed { get; set; }
        public int? passed { get; set; }
        public int? total { get; set; }
        public Decimal? percent { get; set;  }

        // the following fields are used for component type data display
        public int? yes { get; set; }
        public int? no { get; set; }
        public int? na { get; set; }
        public int? alt { get; set; }
        public int? unanswered { get; set; }
    }

}

