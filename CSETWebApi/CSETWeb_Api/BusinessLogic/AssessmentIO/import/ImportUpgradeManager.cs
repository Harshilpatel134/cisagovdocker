using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSETWeb_Api.BusinessLogic.ImportAssessment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSETWeb_Api.BusinessLogic.BusinessManagers
{
    /// <summary>
    /// Applies incremental upgrades to bring older exported 
    /// data up to date.
    /// </summary>
    public class ImportUpgradeManager
    {
        /// <summary>
        /// The list of versions for which incremental updates are supported.
        /// </summary>
        static Dictionary<string, ICSETJSONFileUpgrade> upgraders = new Dictionary<string, ICSETJSONFileUpgrade>();

        static ImportUpgradeManager()
        {
            upgraders.Add("9.0", new CSET90_to_901Upgrade());
            upgraders.Add("9.0.1", new CSET901_to_92Upgrade());
            upgraders.Add("9.0.4", new CSET901_to_92Upgrade());
            upgraders.Add("9.2", null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string Upgrade(string json)
        {
            // Determine the latest version supported by this upgrader
            List<System.Version> knownVersions = new List<System.Version>();
            foreach (var u in upgraders)
            {
                knownVersions.Add(System.Version.Parse(u.Key));
            }
            System.Version latestVersion = knownVersions.Max(x => x);


            // Determine the version of the data
            JObject j = JObject.Parse(json);
            JToken versionToken = j.SelectToken("jCSET_VERSION[0].Version_Id");
            if (versionToken == null)
            {
                throw new ApplicationException("Version could not be identifed corrupted assessment json");
            }
            System.Version version = ConvertFromStringToVersion(versionToken.Value<string>());


            while (version < latestVersion)
            {
                ICSETJSONFileUpgrade fileUpgrade = upgraders[version.ToString()];
                if (fileUpgrade != null)
                {
                    json = fileUpgrade.ExecuteUpgrade(json);
                    version = fileUpgrade.GetVersion();
                }
            }
            return json;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private System.Version ConvertFromStringToVersion(String v)
        {
            // The version string may come in from a float and not be interpreted correctly.  
            // This attempts to turn "9.04" into "9.0.4"
            string[] parts = v.Split(".".ToCharArray());
            if (parts.Length > 1 && parts[1].StartsWith("0"))
            {
                parts[1] = "0." + parts[1].Substring(1);
            }
            v = String.Join(".", parts);
            if (v.EndsWith("."))
            {
                v = v.TrimEnd('.');
            }


            if (int.TryParse(v, out int version))
            {
                return new System.Version(version, 0);
            }
            return new System.Version(v);
        }
    }
}


