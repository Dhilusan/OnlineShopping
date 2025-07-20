using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OnlineShopping.DataAccess.Repository;
using OnlineShopping.DataAccess.Repository.IRepository;
using OnlineShopping.Models;
using OnlineShopping.Models.ViewModels;
using OnlineShopping.Utility;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlineShoppingWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
    public class OrderController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult OrderSummary()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                Order = _unitOfWork.Order.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.Order.Id == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult UpdateOrderDetail()
        {
            var orderFromDb = _unitOfWork.Order.Get(u => u.Id == OrderVM.Order.Id);
            orderFromDb.Name = OrderVM.Order.Name;
            orderFromDb.PhoneNo = OrderVM.Order.PhoneNo;
            orderFromDb.Address = OrderVM.Order.Address;
            orderFromDb.City = OrderVM.Order.City;
            orderFromDb.State = OrderVM.Order.State;
            orderFromDb.PostalCode = OrderVM.Order.PostalCode;

            orderFromDb.ManufactureFromDate = OrderVM.Order.ManufactureFromDate;
            orderFromDb.ManufactureToDate = OrderVM.Order.ManufactureToDate;
            if (!string.IsNullOrEmpty(OrderVM.Order.Carrier))
            {
                orderFromDb.Carrier = OrderVM.Order.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.Order.TrakingNumber))
            {
                orderFromDb.Carrier = OrderVM.Order.TrakingNumber;

            }
            _unitOfWork.Order.Update(orderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_Pay_Now()
        {
            OrderVM.Order = _unitOfWork.Order.Get(u => u.Id == OrderVM.Order.Id,
                includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.
                GetAll(u => u.OrderId == OrderVM.Order.Id, includeProperties: "Product");

            // Payment logic
            var domain = "https://localhost:44393/";

            var options = new SessionCreateOptions
            {

                SuccessUrl = domain + $"customer/card/OrderConfirmation?id={OrderVM.Order.Id}",
                CancelUrl = domain + "customer/card/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var SessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "lkr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.ProductCount
                };
                options.LineItems.Add(SessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Order.UpdateStripePaymentID(OrderVM.Order.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.Order.Id });
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult StartProcessing()
        {

            _unitOfWork.Order.UpdateStatus(OrderVM.Order.Id, SD.StatusProcessing);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.Order.Id });
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelOrder()
        { 
            var order = _unitOfWork.Order.Get(u=>u.Id == OrderVM.Order.Id);

            _unitOfWork.Order.UpdateStatus(order.Id, SD.StatusCancelled,SD.StatusCancelled);
            _unitOfWork.Save();
            TempData["Success"] = "Order cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.Order.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult ShippedOrder()
        {
            var order = _unitOfWork.Order.Get(u => u.Id == OrderVM.Order.Id);

            _unitOfWork.Order.UpdateStatus(order.Id, SD.StatusShipped, SD.PaymentStatusApproved);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.Order.Id });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Order> objOrder;
            if (User.IsInRole(SD.Role_Admin))
            {
                objOrder = _unitOfWork.Order.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrder = _unitOfWork.Order.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }
            switch (status)
            {
                case "pending":
                    objOrder = objOrder.Where(u => u.OrderStatus == SD.PaymentStatusPending);
                    break;
                case "cancelled":
                    objOrder = objOrder.Where(u => u.OrderStatus == SD.StatusCancelled);
                    break;
                case "completed":
                    objOrder = objOrder.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrder = objOrder.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = objOrder });
        }


        
        #endregion
    }
}
