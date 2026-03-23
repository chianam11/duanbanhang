using ShopMVC.Controllers.Api;
using ShopMVC.Data;
using ShopMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShopMVC.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var context = new AppDbContext(options);

            // Seed test data
            context.DanhMucs.Add(new DanhMuc { Id = 1, Ten = "Test Category", HienThi = true });
            context.ThuongHieus.Add(new ThuongHieu { Id = 1, Ten = "Test Brand", HienThi = true });
            context.SanPhams.Add(new SanPham
            {
                Id = 1,
                Ten = "Test Product",
                MoTaNgan = "Test Description",
                Gia = 100000,
                IsActive = true,
                IdDanhMuc = 1,
                IdThuongHieu = 1
            });
            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProducts();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<SanPham>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<SanPham>>(okResult.Value);
            Assert.Single(products);
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_WhenExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProduct(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<SanPham>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var product = Assert.IsType<SanPham>(okResult.Value);
            Assert.Equal(1, product.Id);
            Assert.Equal("Test Product", product.Ten);
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProduct(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetProducts_FiltersBySearch()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProducts(search: "Test");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<SanPham>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<SanPham>>(okResult.Value);
            Assert.Single(products);
        }

        [Fact]
        public async Task GetProducts_FiltersByCategory()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProducts(categoryId: 1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<SanPham>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<SanPham>>(okResult.Value);
            Assert.Single(products);
        }
    }
}