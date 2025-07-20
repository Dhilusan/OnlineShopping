using OnlineShopping.DataAccess.Data;
using OnlineShopping.DataAccess.Repository.IRepository;
using OnlineShopping.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShopping.DataAccess.Repository
{
    public class ShoppingCardRepository : Repository<ShoppingCard>, IShoppingCardRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCardRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(ShoppingCard obj)
        {
            _db.ShoppingCards.Update(obj);
        }
    }
}
