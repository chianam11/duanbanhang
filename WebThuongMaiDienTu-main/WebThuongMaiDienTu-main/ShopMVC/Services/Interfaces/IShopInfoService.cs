namespace ShopMVC.Services.Interfaces
{
    public interface IShopInfoService
    {
        Task<string> GetShopNameAsync(CancellationToken cancellationToken = default);
        Task UpdateShopNameAsync(string shopName, CancellationToken cancellationToken = default);
    }
}
