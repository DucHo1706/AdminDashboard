using System;

namespace AdminDashboard.Models
{
    public class ChatMessage
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}