using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopMVC.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SanPham>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null)
        {
            var query = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.ThuongHieu)
                .Include(p => p.Anhs)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Ten.Contains(search) || (p.MoTaNgan != null && p.MoTaNgan.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.IdDanhMuc == categoryId);
            }

            var totalItems = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = products,
                pagination = new
                {
                    page,
                    pageSize,
                    totalItems,
                    totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                }
            });
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SanPham>> GetProduct(int id)
        {
            var product = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.ThuongHieu)
                .Include(p => p.Anhs)
                .Include(p => p.ChiTietSanPhams)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "QuanTri,NhanVien")]
        public async Task<ActionResult<SanPham>> CreateProduct(SanPham product)
        {
            product.NgayTao = DateTime.Now;
            product.NgayCapNhat = DateTime.Now;

            _context.SanPhams.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        [Authorize(Roles = "QuanTri,NhanVien")]
        public async Task<IActionResult> UpdateProduct(int id, SanPham product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            product.NgayCapNhat = DateTime.Now;

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "QuanTri")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.SanPhams.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = false;
            product.NgayCapNhat = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.SanPhams.Any(e => e.Id == id && e.IsActive);
        }
    }
}
