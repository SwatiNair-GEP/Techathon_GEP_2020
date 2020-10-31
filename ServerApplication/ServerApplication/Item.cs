using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class Item
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
    }

    public class FindSupplierRequestModel
    {
        public int item_id { get; set; }

    }


}
