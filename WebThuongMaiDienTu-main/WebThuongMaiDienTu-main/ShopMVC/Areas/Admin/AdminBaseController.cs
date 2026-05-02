using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopMVC.Configuration;

namespace ShopMVC.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = AppConstants.ROLES_ADMIN_OR_STAFF)]
    public abstract class AdminBaseController : Controller
    {
    }
}
