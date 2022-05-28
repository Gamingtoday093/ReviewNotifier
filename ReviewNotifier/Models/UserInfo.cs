using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewNotifier.Models
{
    public class UserInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string steamId { get; set; }
        public int? avatarImageId { get; set; }
        public string color { get; set; }
        public DateTime createDate { get; set; }
    }
}
