using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewNotifier.Models
{
    public class Seller : UserInfo
    {
        public bool IsPayPalEnabled { get; set; }
        public string PayPalAddress { get; set; }
        public bool IsNanoEnabled { get; set; }
        public string NanoAddress { get; set; }
        public string DiscordWebhookUrl { get; set; }
        public string TermsAndConditions { get; set; }
        public bool IsVerifiedSeller { get; set; }
    }
}
