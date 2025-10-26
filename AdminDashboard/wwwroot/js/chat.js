// --- Kết nối SignalR ---
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .withAutomaticReconnect()
    .build();

const users = {};
let currentReceiverId = null;
const currentUserId = document.getElementById("currentUserId")?.value || "me";

// --- Khi danh sách người dùng cập nhật ---
connection.on("UpdateUserList", (userMap) => {
    const userList = document.getElementById("userList");
    userList.innerHTML = "";

    Object.entries(userMap).forEach(([id, name]) => {
        // Không hiển thị chính mình
        if (id === currentUserId) return;

        const li = document.createElement("li");
        li.className = "list-group-item list-group-item-action";
        li.textContent = name;
        li.style.cursor = "pointer";

        // 👇 Gắn sự kiện click để chọn người nhận
        li.addEventListener("click", () => {
            currentReceiverId = id;
            document.querySelectorAll("#userList .list-group-item").forEach(el => el.classList.remove("active"));
            li.classList.add("active");

            const chatBox = document.getElementById("chatBox");
            chatBox.innerHTML = `<div class='text-muted small'>Đang chat với <strong>${name}</strong></div>`;
        });

        userList.appendChild(li);
    });
});

// --- Khi nhận tin nhắn ---
connection.on("ReceivePrivateMessage", (senderId, senderDisplay, message, utcTime) => {
    const chatBox = document.getElementById("chatBox");
    const msgDiv = document.createElement("div");
    msgDiv.classList.add("my-1");

    const localTime = new Date(utcTime).toLocaleTimeString();
    if (senderId === currentUserId)
        msgDiv.innerHTML = `<div class="text-end text-primary"><small>Bạn • ${localTime}</small><div>${escapeHtml(message)}</div></div>`;
    else
        msgDiv.innerHTML = `<div class="text-start"><small>${escapeHtml(senderDisplay)} • ${localTime}</small><div>${escapeHtml(message)}</div></div>`;

    chatBox.appendChild(msgDiv);
    chatBox.scrollTop = chatBox.scrollHeight;
});

// --- Gửi tin ---
document.getElementById("sendButton").addEventListener("click", () => {
    const text = document.getElementById("messageInput").value.trim();
    if (!text || !currentReceiverId)
        return alert("Chọn người nhận và nhập tin nhắn!");

    connection.invoke("SendPrivateMessage", currentReceiverId, text)
        .catch(err => console.error(err.toString()));

    document.getElementById("messageInput").value = "";
});

// --- Hàm thoát ký tự HTML ---
function escapeHtml(text) {
    return text.replace(/[&<>"'\/]/g, s => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;',
        '"': '&quot;', "'": '&#39;', '/': '&#x2F;'
    }[s]));
}

// --- Kết nối ---
connection.start()
    .then(() => console.log("✅ SignalR connected"))
    .catch(err => console.error("❌ SignalR error:", err));
