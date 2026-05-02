using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Configuration;
using ShopMVC.Data;
using ShopMVC.Models;
using System.Security.Claims;

namespace ShopMVC.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;
        private static readonly string _adminGroup = "Admins";

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        private bool IsSupportAgent()
            => Context.User?.IsInRole(AppConstants.ROLE_ADMIN) == true
            || Context.User?.IsInRole(AppConstants.ROLE_CHAT_SUPPORT) == true;

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var wantsSupportConsole = httpContext?.Request.Query["isAdmin"].ToString() == "true";

            if (wantsSupportConsole && !IsSupportAgent())
            {
                Context.Abort();
                return;
            }

            if (IsSupportAgent() && wantsSupportConsole)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, _adminGroup);
            }
            else
            {
                var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessageFromUser(int sessionId, string message)
        {
            var httpContext = Context.GetHttpContext();
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId) || IsSupportAgent()) return;
            if (string.IsNullOrWhiteSpace(message)) return;

            ChatSession? session = null;

            if (sessionId <= 0)
            {
                session = await _db.ChatSessions
                    .FirstOrDefaultAsync(s => s.UserId == userId && !s.DaDong);

                if (session == null)
                {
                    var productIdStr = httpContext?.Request.Query["productId"].ToString();
                    int? sanPhamId = null;
                    if (int.TryParse(productIdStr, out var pId)) sanPhamId = pId;

                    session = new ChatSession
                    {
                        UserConnectionId = Context.ConnectionId,
                        UserId = userId,
                        SanPhamId = sanPhamId,
                        ThoiGianTao = DateTime.Now,
                        DaDong = false
                    };

                    _db.ChatSessions.Add(session);
                    await _db.SaveChangesAsync();

                    var displayName = Context.User?.Identity?.Name ?? "Khách hàng";
                    await Clients.Group(_adminGroup)
                        .SendAsync("NewUserConnected", session.Id, Context.ConnectionId, displayName);

                    await Clients.Caller.SendAsync("ReceiveSessionId", session.Id);
                }
                else
                {
                    session.UserConnectionId = Context.ConnectionId;
                    await Clients.Caller.SendAsync("ReceiveSessionId", session.Id);
                }
            }
            else
            {
                session = await _db.ChatSessions.FindAsync(sessionId);
            }

            if (session == null) return;

            var msg = new ChatMessage
            {
                ChatSessionId = session.Id,
                NoiDung = message,
                Sender = SenderType.User,
                ThoiGian = DateTime.Now
            };

            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            await Clients.Group(_adminGroup).SendAsync("ReceiveMessage", session.Id, "User", message);
        }

        public async Task SendMessageFromAdmin(int sessionId, string userConnectionId, string message)
        {
            if (!IsSupportAgent()) return;
            if (string.IsNullOrWhiteSpace(message)) return;

            var msg = new ChatMessage
            {
                ChatSessionId = sessionId,
                NoiDung = message,
                Sender = SenderType.Admin,
                ThoiGian = DateTime.Now
            };

            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            var session = await _db.ChatSessions.FindAsync(sessionId);

            if (session != null && !string.IsNullOrEmpty(session.UserId))
            {
                await Clients.Group(session.UserId).SendAsync("ReceiveMessage", sessionId, "Admin", message);
            }
            else if (!string.IsNullOrWhiteSpace(userConnectionId))
            {
                await Clients.Client(userConnectionId).SendAsync("ReceiveMessage", sessionId, "Admin", message);
            }

            await Clients.Caller.SendAsync("ReceiveMessage", sessionId, "Admin", message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var wantsSupportConsole = httpContext?.Request.Query["isAdmin"].ToString() == "true";

            if (IsSupportAgent() && wantsSupportConsole)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, _adminGroup);
            }
            else
            {
                var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
