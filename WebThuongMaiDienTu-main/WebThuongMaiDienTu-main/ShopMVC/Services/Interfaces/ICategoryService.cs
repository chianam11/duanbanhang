using ShopMVC.Models;

namespace ShopMVC.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<DanhMuc>> GetActiveCategoriesAsync();
        void ClearCategoriesCache();
    }
}
