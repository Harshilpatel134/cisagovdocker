//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSET_Main.Questions.POCO;
using DataLayerCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSET_Main.Questions.Slice
{
    public class RequirementsPocoBuilder
    {
        public Dictionary<int, Requirement_And_Set> Requirements { get; private set; }
        

        private Dictionary<String, List<QuestionPoco>> dictionaryCNSSICategory;        
        private Boolean createQuestionPoco;
        private CSET_Context DataContext { get; }

        public RequirementsPocoBuilder(CSET_Context datacontext)
        {
            this.DataContext = datacontext;
            this.dictionaryCNSSICategory = new Dictionary<string, List<QuestionPoco>>();          
        }

        public void BuildRequirementQuestionPocos(UNIVERSAL_SAL_LEVEL selectedSalLevel, List<SETS> listActiveStandards)
        {
            createQuestionPoco = true;
            InitAndBuildRequirementQuestionPocos(selectedSalLevel, listActiveStandards);            
        }

        

        private void InitAndBuildRequirementQuestionPocos(UNIVERSAL_SAL_LEVEL sal, List<SETS> sets)
        {
            Requirements = new Dictionary<int, Requirement_And_Set>();
            dictionaryCNSSICategory.Clear();           
        }
       

        private void SetCNSSIQuestionNumbers()
        {
            foreach (List<QuestionPoco> listQuestions in dictionaryCNSSICategory.Values)
            {
                int i = 1;
                foreach (QuestionPoco qp in listQuestions.OrderBy(x=>x.Question_or_Requirement_ID))
                {
                    qp.CNSSINumber = i;
                    i++;
                }
            }
        }

        private void AddCNSSIQuestions(Requirement_And_Set requirementSet)
        {
            if (requirementSet.IsCNSSI)
            {
                List<QuestionPoco> listQuestionCNSSI;

                if (!dictionaryCNSSICategory.TryGetValue(requirementSet.Question.Category, out listQuestionCNSSI))
                {
                    listQuestionCNSSI = new List<QuestionPoco>();
                    dictionaryCNSSICategory.Add(requirementSet.Question.Category, listQuestionCNSSI);

                }
                listQuestionCNSSI.Add(requirementSet.Question);
            }
        }
    }
}


