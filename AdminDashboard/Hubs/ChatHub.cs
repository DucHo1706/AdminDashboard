using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text;

namespace AdminDashboard.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ADMIN_ID = "admin@gmail.com"; // ⚠️ Thay bằng email/tên đăng nhập admin thật

        public ChatHub(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            var displayName = Context.User?.Identity?.Name ?? userId;

            _onlineUsers[userId] = displayName;

            // Mỗi user sẽ vào group riêng của họ
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Nếu là admin -> vào group admin để nhận tất cả tin nhắn
            if (userId == ADMIN_ID)
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

            await Clients.All.SendAsync("UpdateUserList", _onlineUsers.ToDictionary(x => x.Key, x => x.Value));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            _onlineUsers.TryRemove(userId, out _);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");

            await Clients.All.SendAsync("UpdateUserList", _onlineUsers.ToDictionary(x => x.Key, x => x.Value));
            await base.OnDisconnectedAsync(exception);
        }

        // ===================== GỬI TIN NHẮN =====================
        public async Task SendPrivateMessage(string receiverUserId, string message)
        {
            var senderUserId = Context.UserIdentifier ?? Context.ConnectionId;
            var senderDisplay = Context.User?.Identity?.Name ?? senderUserId;
            var timestamp = DateTime.Now;

            // Nếu người gửi KHÔNG phải admin thì chỉ gửi cho admin
            if (senderUserId != ADMIN_ID)
            {
                await Clients.Group("admins")
                    .SendAsync("ReceivePrivateMessage", senderUserId, senderDisplay, message, timestamp);
            }
            else
            {
                // Nếu admin gửi -> gửi riêng cho user đó
                await Clients.Group($"user_{receiverUserId}")
                    .SendAsync("ReceivePrivateMessage", senderUserId, senderDisplay, message, timestamp);
            }

            // Gửi lại cho người gửi để hiển thị tin của mình
            await Clients.Caller.SendAsync("ReceivePrivateMessage", senderUserId, senderDisplay, message, timestamp);

            // Lưu Firebase
            await SaveMessageToFirebase(senderUserId, receiverUserId, senderDisplay, message, timestamp);
        }

        // ===================== 🔥 LƯU FIREBASE =====================
        private async Task SaveMessageToFirebase(string senderId, string receiverId, string senderName, string message, DateTime timestamp)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var firebaseUrl = "https://chathub-46d8f-default-rtdb.firebaseio.com/messages.json";

                var messageData = new
                {
                    senderId,
                    receiverId,
                    senderName,
                    message,
                    timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var json = JsonSerializer.Serialize(messageData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await httpClient.PostAsync(firebaseUrl, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Lỗi Firebase: {ex.Message}");
            }


        }
        public async Task SendMessage(string user, string message)
        {
            // Gửi tin đến tất cả client đang kết nối (admin + user)
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
