using OnlineShopping.Models;
using OnlineShopping.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShopping.DataAccess.Repository.IRepository
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order obj);

        
        void UpdateStripePaymentID(int id, string sessionId, string paymentId);

        void UpdateStatus(int id, string orderStatus, string? paymentStatus = SD.PaymentStatusApproved);
    }
}
