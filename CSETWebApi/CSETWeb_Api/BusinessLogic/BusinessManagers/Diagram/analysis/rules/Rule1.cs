﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Helpers;
using CSETWeb_Api.BusinessLogic.BusinessManagers.Diagram.Analysis;
using CSETWeb_Api.BusinessManagers;
using CSETWeb_Api.BusinessManagers.Diagram.Analysis;

namespace CSETWeb_Api.BusinessLogic.BusinessManagers.Diagram.analysis.rules
{
    internal class Rule1 : AbstractRule, IRuleEvaluate
    {
        private Dictionary<String, NetworkComponent> nodes = new Dictionary<string, NetworkComponent>();
        private List<NetworkLink> Links = new List<NetworkLink>();
        private List<NetworkAnalysisNode> ListAnalysisNodes = new List<NetworkAnalysisNode>();
        private Dictionary<string, NetworkLayer> layers = new Dictionary<string, NetworkLayer>();
        //drawio id to zone lookup
        private Dictionary<string, NetworkZone> zones = new Dictionary<string, NetworkZone>();               
        private List<IDiagramAnalysisNodeMessage> NetworkWarnings = new List<IDiagramAnalysisNodeMessage>();


        private String rule1 = "The network path identified by the components, {0} and {1}, appears to connect network " +
            "segments whose components reside in different zones.  A firewall to filter the traffic on this path is " +
            "recommended to protect the components in one zone from a compromised component in the other zone.";

        public Rule1(SimplifiedNetwork simplifiedNetwork)
        {
            this.nodes = simplifiedNetwork.Nodes;
        }

        public List<IDiagramAnalysisNodeMessage> Evaluate()
        {
            checkRule1();
            return this.Messages;
        }


        private HashSet<String> NotYetVisited = new HashSet<string>();

        private void checkRule1()
        {
            List<int> allowToConnect = new List<int>
            {
                Constants.FIREWALL,
                Constants.UNIDIRECTIONAL_DEVICE,
                Constants.IDS_TYPE,
                Constants.SERIAL_RADIO
            };

            //check all of a components immediate connections
            //if it is outside of it's zone and it does not connect to a firewall 
            //then we need to flag this component     
            List<int> exempt = new List<int>
            {
                Constants.WEB_TYPE,
                Constants.VENDOR_TYPE,
                Constants.PARTNER_TYPE,
                Constants.FIREWALL,
                Constants.UNIDIRECTIONAL_DEVICE,
                Constants.IDS_TYPE
            };

            var suspectslist = nodes.Values.Where(x => !exempt.Contains(x.Component_Symbol_Id));
            foreach (var node in suspectslist)
            {
                if (NotYetVisited.Add(node.ID))
                {
                    foreach (var child in node.Connections)
                    {
                        if (!node.IsInSameZone(child))
                        {
                            if (!allowToConnect.Contains(child.Component_Symbol_Id))
                            {
                                String text = String.Format(rule1, node.ComponentName, child.ComponentName).Replace("\n", " ");
                                SetLineMessage(node, child, text);
                            }
                        }
                    }
                }
            }
        }
    }
}
