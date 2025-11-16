using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[Route("api/chat")]
[ApiController]
public class ChatApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatApiController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // 🔹 Lấy danh sách user + tin chưa đọc
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var httpClient = _httpClientFactory.CreateClient();
        var firebaseUrl = "https://chathub-46d8f-default-rtdb.firebaseio.com/messages.json";
        var res = await httpClient.GetAsync(firebaseUrl);

        if (!res.IsSuccessStatusCode)
            return BadRequest("Không lấy được dữ liệu Firebase");

        var json = await res.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json).RootElement;

        var result = new List<object>();

        foreach (var chat in data.EnumerateObject())
        {
            var userId = chat.Name.Replace("_admin", "");
            int unreadCount = chat.Value.EnumerateObject()
                .Count(m => m.Value.GetProperty("receiverId").GetString() == "admin"
                         && m.Value.GetProperty("isRead").GetBoolean() == false);

            result.Add(new
            {
                userId,
                unreadCount
            });
        }

        // 🔸 Sắp xếp: user nào có tin chưa đọc thì lên đầu
        var sorted = result.OrderByDescending(x => ((int)x.GetType().GetProperty("unreadCount")?.GetValue(x)! > 0)).ToList();
        return Ok(sorted);
    }

    // 🔹 Lấy lịch sử chat theo user
    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetChatHistory(string userId)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var firebaseUrl = $"https://chathub-46d8f-default-rtdb.firebaseio.com/messages/{userId}_admin.json";
        var res = await httpClient.GetAsync(firebaseUrl);

        if (!res.IsSuccessStatusCode)
            return BadRequest("Không lấy được lịch sử chat");

        var json = await res.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }

    // 🔹 Cập nhật tin nhắn đã đọc
    [HttpPost("mark-read/{userId}")]
    public async Task<IActionResult> MarkAsRead(string userId)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var firebaseUrl = $"https://chathub-46d8f-default-rtdb.firebaseio.com/messages/{userId}_admin.json";

        var res = await httpClient.GetAsync(firebaseUrl);
        if (!res.IsSuccessStatusCode) return BadRequest();

        var json = await res.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json).RootElement;

        foreach (var msg in data.EnumerateObject())
        {
            var key = msg.Name;
            var msgUrl = $"{firebaseUrl}/{key}/isRead.json";
            var content = new StringContent("true", Encoding.UTF8, "application/json");
            await httpClient.PutAsync(msgUrl, content);
        }

        return Ok();
    }
}
