using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewNotifier.Models
{
    public class DisplayProduct
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string githubUrl { get; set; }
        public int imageId { get; set; }
        public decimal price { get; set; }
        public string category { get; set; }
        public bool isLoaderEnabled { get; set; }
        public bool isEnabled { get; set; }
        public DateTime lastUpdate { get; set; }
        public DateTime createDate { get; set; }

        public Seller seller { get; set; }

        public int totalDownloadsCount { get; set; }
        public byte averageRating { get; set; }
        public int ratingsCount { get; set; }

        public UserInfo customer { get; set; }

        public List<object> tabs { get; set; }
        public List<object> media { get; set; }
        public List<ProductReview> reviews { get; set; }
        public List<object> branches { get; set; }
    }
}
