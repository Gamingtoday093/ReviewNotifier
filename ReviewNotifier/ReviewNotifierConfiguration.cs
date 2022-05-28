using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReviewNotifier
{
    public class ReviewNotifierConfiguration : IRocketPluginConfiguration
    {
        public string UsernameOrSteamId { get; set; }
        public float FuzzySearchSensitivity { get; set; }
        [XmlArrayItem("ProductID")]
        public int[] ProductIDs { get; set; }
        [XmlArrayItem("ProductID")]
        public int[] IgnoreProductIDs { get; set; }
        public void LoadDefaults()
        {
            UsernameOrSteamId = "Your UnturnedStore Username Or SteamId";
            FuzzySearchSensitivity = 85f;
            ProductIDs = new int[]
            {
                255
            };
            IgnoreProductIDs = new int[]
            {
                216
            };
        }
    }
}
