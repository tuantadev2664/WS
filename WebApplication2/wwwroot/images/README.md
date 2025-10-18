# Ship Images for Mini Battleship - Ship Parts Approach

## Required Images

Để hiển thị tàu bằng các phần riêng biệt, bạn cần tạo các file hình ảnh sau:

### Tàu 2 ô (ship-1):
- **ship-1-start.png** (45×45px) - Đầu tàu (ô đầu tiên)
- **ship-1-end.png** (45×45px) - Đuôi tàu (ô cuối cùng)

### Tàu 3 ô (ship-2):
- **ship-2-start.png** (45×45px) - Đầu tàu (ô đầu tiên)
- **ship-2-middle.png** (45×45px) - Thân tàu (ô giữa)
- **ship-2-end.png** (45×45px) - Đuôi tàu (ô cuối cùng)

### Tàu dọc:
- **ship-1-start-vertical.png** (45×45px) - Đầu tàu dọc
- **ship-1-end-vertical.png** (45×45px) - Đuôi tàu dọc
- **ship-2-start-vertical.png** (45×45px) - Đầu tàu 3 ô dọc
- **ship-2-middle-vertical.png** (45×45px) - Thân tàu 3 ô dọc
- **ship-2-end-vertical.png** (45×45px) - Đuôi tàu 3 ô dọc

## Cách hoạt động

Mỗi ô sẽ hiển thị một phần tàu riêng biệt:
- **Ô đầu tiên**: Hiển thị đầu tàu
- **Ô giữa** (nếu có): Hiển thị thân tàu
- **Ô cuối cùng**: Hiển thị đuôi tàu

## Ưu điểm

- ✅ Không bị che khuất bởi các ô khác
- ✅ Hiển thị chính xác trong từng ô
- ✅ Dễ dàng xử lý tàu bị bắn trúng
- ✅ Hỗ trợ cả tàu ngang và dọc