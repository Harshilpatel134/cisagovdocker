//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSET_Main.Data;
using DataLayerCore.Model;

namespace ResourceLibrary.Nodes
{
    public class TopicNode : ResourceNode
    {
        protected TopicNode(PROCUREMENTLANGUAGEDATA procTopicData)
        {
            TreeTextNode = procTopicData.Topic_Name;
            Type = ResourceNodeType.ProcurementTopic;
            ID = procTopicData.Procurement_Id;
        }

        protected TopicNode(CATALOGRECOMMENDATIONSDATA recommTopicData)
        {
            TreeTextNode = recommTopicData.Topic_Name;
            Type = ResourceNodeType.RecCatalogTopic;
            ID = recommTopicData.Data_Id;
        }
    }
}


