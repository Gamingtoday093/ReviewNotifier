using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewNotifier.Models
{
    public class ProductReview
    {
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public byte rating { get; set; }
        public int productId { get; set; }
        public int userId { get; set; }
        public DateTime lastUpdate { get; set; }
        public DateTime createDate { get; set; }

        public UserInfo user { get; set; }
    }
}
