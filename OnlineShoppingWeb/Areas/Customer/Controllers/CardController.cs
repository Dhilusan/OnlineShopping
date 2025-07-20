using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mono.TextTemplating;
using OnlineShopping.DataAccess.Repository.IRepository;
using OnlineShopping.Models;
using OnlineShopping.Models.ViewModels;
using OnlineShopping.Utility;
using Stripe.Checkout;
using System.Security.AccessControl;
using System.Security.Claims;

namespace OnlineShoppingWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCardVM ShoppingCardVM { get; set; }

        public CardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCardVM = new()
            {
                ShoppingCardList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserID == userId,
                includeProperties: "Product"),
                Order = new()
            };

            foreach (var card in ShoppingCardVM.ShoppingCardList)
            {

                card.Price = GetPriceOnQty(card);
                if (card.Price > 0)
                {
                    double amount = card.Price * card.Count;
                    ShoppingCardVM.Order.OrderTotal += amount;

                }
            }
            return View(ShoppingCardVM);
        }
        private double GetPriceOnQty(ShoppingCard shoppingCard)
        {
            return shoppingCard.Product.Price;
        }
        private int GetBalannceQty(ShoppingCard shoppingCard)
        {
            return shoppingCard.Product.BalanceQty;
        }
        public IActionResult Plus(int cardId)
        {
            var cardFromDb = _unitOfWork.ShoppingCard.Get(u => u.Id == cardId);
            cardFromDb.Count += 1;
            //HttpContext.Session.SetInt32(SD.SessionCrad, _unitOfWork.ShoppingCard.
            //   GetAll(u => u.ApplicationUserID == cardFromDb.ApplicationUserID).Count() + 1);
            _unitOfWork.ShoppingCard.Update(cardFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));


        }
        public IActionResult Minus(int cardId)
        {
            var cardFromDb = _unitOfWork.ShoppingCard.Get(u => u.Id == cardId);
            if (cardFromDb.Count <= 1)
            {
               // HttpContext.Session.SetInt32(SD.SessionCrad, _unitOfWork.ShoppingCard.
               //GetAll(u => u.ApplicationUserID == cardFromDb.ApplicationUserID).Count() - 1);
                _unitOfWork.ShoppingCard.Remove(cardFromDb);
            }
            else
            {
                cardFromDb.Count -= 1;
                _unitOfWork.ShoppingCard.Update(cardFromDb);
            }


            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));


        }
        public IActionResult Remove(int cardId)
        {
            var cardFromDb = _unitOfWork.ShoppingCard.Get(u => u.Id == cardId);
            //HttpContext.Session.SetInt32(SD.SessionCrad, _unitOfWork.ShoppingCard.
            //   GetAll(u => u.ApplicationUserID == cardFromDb.ApplicationUserID).Count() - 1);
            _unitOfWork.ShoppingCard.Remove(cardFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));


        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCardVM = new()
            {
                ShoppingCardList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserID == userId,
                includeProperties: "Product"),
                Order = new()
            };

            ShoppingCardVM.Order.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCardVM.Order.Name = ShoppingCardVM.Order.ApplicationUser.Name;
            ShoppingCardVM.Order.PhoneNo = ShoppingCardVM.Order.ApplicationUser.PhoneNumber;
            ShoppingCardVM.Order.Address = ShoppingCardVM.Order.ApplicationUser.StreetAddress;
            ShoppingCardVM.Order.City = ShoppingCardVM.Order.ApplicationUser.City;
            ShoppingCardVM.Order.State = ShoppingCardVM.Order.ApplicationUser.State;
            ShoppingCardVM.Order.PostalCode = ShoppingCardVM.Order.ApplicationUser.PostalCode;

            foreach (var card in ShoppingCardVM.ShoppingCardList)
            {

                card.Price = GetPriceOnQty(card);
                if (card.Price > 0)
                {
                    double amount = card.Price * card.Count;
                    ShoppingCardVM.Order.OrderTotal += amount;

                }
            }
            return View(ShoppingCardVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost(ShoppingCardVM shoppingCardVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCardVM.ShoppingCardList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserID == userId, includeProperties: "Product");

            ShoppingCardVM.Order.OrderDate = DateTime.Now;
            ShoppingCardVM.Order.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            foreach (var card in ShoppingCardVM.ShoppingCardList)
            {

                card.Price = GetPriceOnQty(card);
                if (card.Price > 0)
                {
                    double amount = card.Price * card.Count;
                    ShoppingCardVM.Order.OrderTotal += amount;

                }
            }

            ShoppingCardVM.Order.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCardVM.Order.OrderStatus = SD.StatusPending;

            _unitOfWork.Order.Add(ShoppingCardVM.Order);
            _unitOfWork.Save();

            foreach (var card in ShoppingCardVM.ShoppingCardList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = card.ProductId,
                    OrderId = ShoppingCardVM.Order.Id,
                    Price = card.Price,
                    ProductCount = card.Count,
                    //SeatingConfiguration = card.SeatingConfiguration,
                    //InteriorDesign = card.InteriorDesign,
                    AdditionalNote = card.AdditionalNote

                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();

                _unitOfWork.Product.UpdateProductQtyBalance(card.ProductId, GetBalannceQty(card) - card.Count);
                _unitOfWork.Save();
            }


            if (true)
            {

                var domain = "https://localhost:44393/";

                var options = new SessionCreateOptions
                {

                    SuccessUrl = domain + $"customer/card/OrderConfirmation?id={ShoppingCardVM.Order.Id}",
                    CancelUrl = domain + "customer/card/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCardVM.ShoppingCardList)
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
                        Quantity = item.Count
                    };
                    options.LineItems.Add(SessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.Order.UpdateStripePaymentID(ShoppingCardVM.Order.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            return View(nameof(OrderConfirmation), new { id = ShoppingCardVM.Order.Id });



        }
        public IActionResult OrderConfirmation(int id)
        {
            Order order = _unitOfWork.Order.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (order.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(order.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Order.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Order.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                    //EmailSender emailSender = new EmailSender();
                    //emailSender.SendEmail("","Test","Test");
                        
                        
                }
            }
            List<ShoppingCard> shoppingCards = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserID == order.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCard.RemoveRange(shoppingCards);
            _unitOfWork.Save();

            return View(nameof(OrderConfirmation));
        }
    }
}
