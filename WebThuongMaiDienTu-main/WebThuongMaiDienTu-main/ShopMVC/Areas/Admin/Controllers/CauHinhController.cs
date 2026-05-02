using Microsoft.AspNetCore.Mvc;
using ShopMVC.Areas.Admin.ViewModels;
using ShopMVC.Services.Interfaces;

namespace ShopMVC.Areas.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "QuanTri")]
    public class CauHinhController : AdminBaseController
    {
        private readonly IShopInfoService _shopInfoService;

        public CauHinhController(IShopInfoService shopInfoService)
        {
            _shopInfoService = shopInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new ShopSettingsViewModel
            {
                ShopName = await _shopInfoService.GetShopNameAsync(cancellationToken)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ShopSettingsViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _shopInfoService.UpdateShopNameAsync(model.ShopName, cancellationToken);
            TempData["toast"] = "Da cap nhat ten shop.";
            return RedirectToAction(nameof(Index));
        }
    }
}
