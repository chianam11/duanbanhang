using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;

namespace ShopMVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;

        public ChatController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var session = await _db.ChatSessions
                .Include(s => s.Messages)
                .Where(s => s.UserId == userId && !s.DaDong)
                .OrderByDescending(s => s.ThoiGianTao)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                return Json(new
                {
                    success = true,
                    sessionId = 0,
                    messages = Array.Empty<object>()
                });
            }

            var messages = session.Messages
                .OrderBy(m => m.ThoiGian)
                .Select(m => new
                {
                    sender = m.Sender.ToString(),
                    message = m.NoiDung,
                    time = m.ThoiGian
                })
                .ToList();

            return Json(new
            {
                success = true,
                sessionId = session.Id,
                messages
            });
        }
    }
}
