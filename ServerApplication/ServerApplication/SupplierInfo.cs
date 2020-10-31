using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class SupplierInfo
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ContactDetails { get; set; }
        public double Score { get; set; }
 
    }

    public class SupplierScoreInfo
    {
        public int SupplierId { get; set; }
        
        public double score { get; set; }

    }
}
