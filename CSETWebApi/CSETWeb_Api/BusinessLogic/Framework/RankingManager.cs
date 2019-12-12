//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using DataLayerCore.Model;
using System;
using System.Linq;

namespace CSET_Main.Framework
{
    public class RankingManager
    {
        private CSET_Context ControlDataRepository;

        public RankingManager(CSET_Context controlDataRepository)
        {
            this.ControlDataRepository = controlDataRepository;
        }


        public int GetMaxQuestionRanking()
        {
            try
            {
                int maxReqRank = Convert.ToInt32(this.ControlDataRepository.NEW_REQUIREMENT.Max(x => x.Ranking).Value);
                int maxQuestionRank = Convert.ToInt32(this.ControlDataRepository.NEW_QUESTION.Max(x => x.Ranking).Value);

                if (maxReqRank > maxQuestionRank)
                    return maxReqRank;
                else
                    return maxQuestionRank;
            }
            catch
            {
                return 10000;
            }
        }
    }
}


