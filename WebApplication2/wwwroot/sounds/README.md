# Sound Effects for Mini Battleship

## Required Sound Files

Để thêm âm thanh cho game, bạn cần tạo các file âm thanh sau:

### 1. hit.mp3 / hit.wav
- **Mô tả**: Âm thanh khi bắn trúng tàu
- **Độ dài**: 1-2 giây
- **Gợi ý**: Tiếng nổ, tiếng va chạm, hoặc âm thanh "boom"

### 2. miss.mp3 / miss.wav  
- **Mô tả**: Âm thanh khi bắn trượt
- **Độ dài**: 1-2 giây
- **Gợi ý**: Tiếng nước, tiếng splash, hoặc âm thanh "splash"

### 3. win.mp3 / win.wav
- **Mô tả**: Âm thanh khi thắng game
- **Độ dài**: 3-5 giây
- **Gợi ý**: Fanfare, victory sound, hoặc âm thanh chiến thắng

### 4. fire.mp3 / fire.wav
- **Mô tả**: Âm thanh khi bắn (optional)
- **Độ dài**: 0.5-1 giây
- **Gợi ý**: Tiếng súng, tiếng pháo, hoặc âm thanh bắn

## Cách thêm âm thanh

1. Tạo các file âm thanh với tên chính xác như trên
2. Đặt vào thư mục `WebApplication2/wwwroot/sounds/`
3. Hỗ trợ định dạng: MP3, WAV, OGG
4. Kích thước file: < 1MB mỗi file để tải nhanh

## Fallback

Nếu không có file âm thanh, game sẽ sử dụng Web Audio API để tạo âm thanh cơ bản.
