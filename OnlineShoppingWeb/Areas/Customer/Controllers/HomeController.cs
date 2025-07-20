using OnlineShopping.DataAccess.Repository.IRepository;
using OnlineShopping.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OnlineShopping.Utility;
using Microsoft.AspNetCore.Http;

namespace OnlineShoppingWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCrad,
                   _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserID == claim.Value).Count());

            }

            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCard card = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(card);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCard shoppingCard)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCard.ApplicationUserID = userId;

            var a = userId;
            var b = shoppingCard.ProductId;

            ShoppingCard cardFromDb = _unitOfWork.ShoppingCard.Get(u => u.ApplicationUserID == userId &&
            u.ProductId == shoppingCard.ProductId);

            if (cardFromDb != null)
            {


                cardFromDb.Count += shoppingCard.Count;
                _unitOfWork.ShoppingCard.Update(cardFromDb);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCard.Add(shoppingCard);
                _unitOfWork.Save();
                
                HttpContext.Session.SetInt32(SD.SessionCrad,
                _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserID == userId).Count());

            }

            TempData["success"] = "Card updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}