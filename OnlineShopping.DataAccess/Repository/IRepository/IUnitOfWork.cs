using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShopping.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IShoppingCardRepository ShoppingCard { get; }
        IApplicationUserRepository ApplicationUser { get; }

        IOrderRepository Order { get; }

        IOrderDetailRepository OrderDetail { get; }
        void Save();
    }
}
