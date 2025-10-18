# 🚀 Mini Battleship Game

Một game Battleship đơn giản và đẹp mắt được xây dựng với ASP.NET Core MVC.

## ✨ Tính năng

- 🎮 **Chế độ chơi đa dạng**: Chơi vs Bot hoặc 2 người
- 🎨 **UI/UX đẹp mắt**: Thiết kế hiện đại với gradient và animation
- ✈️ **Hình ảnh máy bay**: Thay thế ô tàu bằng biểu tượng máy bay
- 🔊 **Sound effects**: Âm thanh khi bắn trúng/trượt và chiến thắng
- 🎆 **Hiệu ứng đặc biệt**: Animation, particle effects và confetti
- 📱 **Responsive**: Tương thích với mọi thiết bị

## 🎯 Cách chơi

1. **Mục tiêu**: Bắn trúng tất cả tàu của đối thủ trước khi họ bắn trúng tàu của bạn
2. **Tàu chiến**: Mỗi người có 3 tàu (2 tàu 2 ô, 1 tàu 3 ô)
3. **Lượt chơi**: Lần lượt bắn vào ô của đối thủ
4. **Kết quả**:
   - 🎯 **Bắn trúng**: Ô chuyển màu đỏ với biểu tượng 💥
   - 🌊 **Bắn trượt**: Ô hiển thị chấm tròn trắng
   - 🏆 **Chiến thắng**: Khi bắn trúng tất cả tàu đối thủ

## 🚀 Cài đặt và chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt

1. **Clone repository**
   ```bash
   git clone https://github.com/tuantadev2664/WS.git
   cd MiniBattleship
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Chạy ứng dụng**
   ```bash
   dotnet run --project WebApplication2
   ```

4. **Truy cập game**
   - Mở trình duyệt và truy cập: `https://localhost:5001` hoặc `http://localhost:5000`

## 🎮 Cách sử dụng

### Trang chủ
- **🤖 Chơi vs Bot**: Chơi với máy tính
- **👥 Chơi 2 người**: Chơi với bạn bè trên cùng một máy

### Trong game
- **🎯 Bảng bắn**: Click vào ô để bắn
- **🚢 Bảng của bạn**: Hiển thị tàu và kết quả bắn của đối thủ
- **🔄 Đổi lượt**: Trong chế độ 2 người

## 🎨 Tính năng UI/UX

### Thiết kế
- **Gradient backgrounds**: Màu sắc đẹp mắt
- **Glass morphism**: Hiệu ứng kính mờ
- **Responsive design**: Tương thích mobile/desktop

### Animation
- **Ship floating**: Tàu bay lơ lửng
- **Hit explosion**: Hiệu ứng nổ khi bắn trúng
- **Miss ripple**: Gợn sóng khi bắn trượt
- **Particle effects**: Hiệu ứng hạt khi bắn trúng

### Sound Effects
- **Bắn trúng**: Âm thanh nổ
- **Bắn trượt**: Âm thanh nước
- **Chiến thắng**: Nhạc fanfare

## 🛠️ Công nghệ sử dụng

- **Backend**: ASP.NET Core MVC
- **Frontend**: HTML5, CSS3, JavaScript ES6+
- **Styling**: Custom CSS với animations
- **Audio**: Web Audio API
- **Responsive**: CSS Grid & Flexbox

## 📁 Cấu trúc dự án

```
WebApplication2/
├── Controllers/
│   └── HomeController.cs          # Controller chính
├── Models/
│   ├── Board.cs                   # Model bảng game
│   ├── Cell.cs                    # Model ô
│   ├── GameState.cs               # Trạng thái game
│   └── ...
├── Services/
│   ├── GameLogic.cs               # Logic game
│   └── HtmlRenderer.cs            # Render HTML
├── Views/
│   ├── Home/
│   │   └── Index.cshtml           # Trang chủ
│   └── Shared/
│       └── _Layout.cshtml         # Layout chính
├── wwwroot/
│   ├── css/
│   │   └── site.css               # CSS chính
│   └── js/
│       └── game.js                # JavaScript game
└── Program.cs                     # Entry point
```

## 🎯 Game Logic

### Board Setup
- Kích thước: 7x7 ô
- Tàu: 3 tàu (2 tàu 2 ô, 1 tàu 3 ô)
- Vị trí: Ngẫu nhiên, không chồng lấp

### Game Flow
1. **Khởi tạo**: Đặt tàu ngẫu nhiên
2. **Lượt chơi**: Bắn vào ô đối thủ
3. **Kết quả**: Cập nhật trạng thái ô
4. **Kiểm tra**: Game over khi tất cả tàu bị bắn trúng

## 🔧 Tùy chỉnh

### Thay đổi kích thước bảng
```csharp
// Trong Models/Board.cs
public const int BoardSize = 7; // Thay đổi số này
```

### Thay đổi số lượng tàu
```csharp
// Trong Services/GameLogic.cs
private static readonly int[] ShipLengths = new[] { 3, 2, 2 }; // Thay đổi mảng này
```

### Tùy chỉnh màu sắc
```css
/* Trong wwwroot/css/site.css */
:root {
  --primary-color: #667eea;
  --secondary-color: #764ba2;
  --success-color: #4ecdc4;
  --danger-color: #ff6b6b;
}
```

## 🐛 Troubleshooting

### Lỗi thường gặp
1. **Port đã được sử dụng**: Thay đổi port trong `launchSettings.json`
2. **CSS không load**: Kiểm tra đường dẫn trong `_Layout.cshtml`
3. **JavaScript không hoạt động**: Kiểm tra console browser

### Debug
- Sử dụng F12 Developer Tools
- Kiểm tra Console tab để xem lỗi JavaScript
- Kiểm tra Network tab để xem request/response

## 📝 License

MIT License - Xem file LICENSE để biết thêm chi tiết.

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push to branch
5. Tạo Pull Request

## 📞 Support

Nếu gặp vấn đề, hãy tạo issue trên GitHub repository.

---

**Made with ❤️ by [Your Name]**
