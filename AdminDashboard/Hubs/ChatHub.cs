using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AdminDashboard.Hubs
{
    public class ChatHub : Hub
    {
        // Danh sách người dùng online: <UserId, DisplayName>
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            var displayName = Context.User?.Identity?.Name ?? userId;

            _onlineUsers[userId] = displayName;

            // Thêm user này vào nhóm riêng của họ
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Gửi danh sách user cập nhật cho tất cả
            await Clients.All.SendAsync("UpdateUserList", _onlineUsers.ToDictionary(x => x.Key, x => x.Value));

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            _onlineUsers.TryRemove(userId, out _);

            // Rời khỏi nhóm
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Gửi danh sách mới sau khi có người rời
            await Clients.All.SendAsync("UpdateUserList", _onlineUsers.ToDictionary(x => x.Key, x => x.Value));

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(string receiverUserId, string message)
        {
            var senderUserId = Context.UserIdentifier ?? Context.ConnectionId;
            var senderDisplay = Context.User?.Identity?.Name ?? senderUserId;

            // Gửi cho người nhận (trong group riêng)
            await Clients.Group($"user_{receiverUserId}")
                .SendAsync("ReceivePrivateMessage", senderUserId, senderDisplay, message, DateTime.Now);

            // Gửi lại cho người gửi để hiển thị tin nhắn của mình
            await Clients.Caller.SendAsync("ReceivePrivateMessage", senderUserId, senderDisplay, message, DateTime.Now);
        }

    }
}
