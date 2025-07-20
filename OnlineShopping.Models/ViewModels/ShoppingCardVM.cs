using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShopping.Models.ViewModels
{
    public class ShoppingCardVM
    {
        public IEnumerable<ShoppingCard> ShoppingCardList { get; set; }
        public Order Order { get; set; }
        //public double OrderTotal { get; set; }
    }
}
