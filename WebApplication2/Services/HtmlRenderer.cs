using MiniBattleship.Models;
using System.Net;
using System.Text;

namespace MiniBattleship.Services
{
    public static class HtmlRenderer
    {
        public static string HtmlPage(GameState g, string flash = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Mini Battleship</title>");
            sb.AppendLine("<link rel='stylesheet' href='/css/site.css'>");
            sb.AppendLine("<script src='/js/game.js'></script>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<div class='battleship-game'>");
            sb.AppendLine("<div class='game-header'>");
            sb.AppendLine("<h1 class='game-title'>🚀 Mini Battleship</h1>");
            sb.AppendLine("</div>");


            if (!string.IsNullOrEmpty(flash)) 
            {
                string flashClass = flash.Contains("trúng") ? "success" : flash.Contains("trượt") ? "error" : "info";
                sb.AppendLine($"<div class='flash {flashClass}'>{WebUtility.HtmlEncode(flash)}</div>");
            }

            if (g.IsGameOver)
            {
                sb.AppendLine("<div class='winner-modal'>");
                sb.AppendLine("<div class='winner-content'>");
                sb.AppendLine($"<h2>🎉 {WebUtility.HtmlEncode(g.Winner)} Thắng! 🎉</h2>");
                sb.AppendLine($"<p>📊 Thống kê: {WebUtility.HtmlEncode(g.A.Name)}: {g.ShotsTakenA} phát | {WebUtility.HtmlEncode(g.B.Name)}: {g.ShotsTakenB} phát</p>");
                sb.AppendLine("<div class='winner-actions'>");
                sb.AppendLine("<a class='btn btn-primary' href='/new?mode=bot'>🤖 Chơi lại vs Bot</a>");
                sb.AppendLine("<a class='btn btn-secondary' href='/new?mode=hotseat'>👥 Chơi lại 2 người</a>");
                sb.AppendLine("</div>");
                sb.AppendLine("</div>");
                sb.AppendLine("</div>");
                sb.AppendLine(RenderBoards(g, showFireControls: false));
                sb.AppendLine("</div>");
                sb.AppendLine("</body></html>");
                return sb.ToString();
            }


            sb.AppendLine("<div class='game-info'>");
            sb.AppendLine($"<div class='game-mode'>🎮 Chế độ: {g.Mode}</div>");
            sb.AppendLine($"<div class='game-turn'>⚡ Lượt: {g.Active}</div>");
            sb.AppendLine($"<div class='game-stats'>");
            sb.AppendLine($"<span>📊 {WebUtility.HtmlEncode(g.A.Name)}: {g.ShotsTakenA} phát</span>");
            sb.AppendLine($"<span>📊 {WebUtility.HtmlEncode(g.B.Name)}: {g.ShotsTakenB} phát</span>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='game-actions'>");
            sb.AppendLine("<a class='btn btn-primary' href='/new?mode=bot'>🤖 Game mới vs Bot</a>");
            sb.AppendLine("<a class='btn btn-secondary' href='/new?mode=hotseat'>👥 Game mới 2 người</a>");
            sb.AppendLine("</div>");

            bool showFire = true;
            sb.AppendLine(RenderBoards(g, showFire));

            if (g.Mode == Mode.HotSeat)
            {
                sb.AppendLine("<div class='game-actions'>");
                sb.AppendLine("<button class='btn btn-secondary switch-turn-btn'>🔄 Đổi lượt</button>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string RenderBoards(GameState g, bool showFireControls)
        {
            var sb = new StringBuilder();
            var current = g.Active == Turn.A ? g.A : g.B;
            var opponent = g.Active == Turn.A ? g.B : g.A;

            sb.AppendLine("<div class='boards-container'>");

            sb.AppendLine($"<div class='board-section'>");
            sb.AppendLine($"<h3 class='board-title'>🎯 Bảng bắn – {WebUtility.HtmlEncode(opponent.Name)}</h3>");
            sb.AppendLine(RenderGrid(opponent, currentIsAttacker: true, showFireControls));
            sb.AppendLine("</div>");

            sb.AppendLine($"<div class='board-section'>");
            sb.AppendLine($"<h3 class='board-title'>🚢 Bảng của bạn – {WebUtility.HtmlEncode(current.Name)}</h3>");
            sb.AppendLine(RenderOwn(current.Own));
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private static string RenderGrid(PlayerState defender, bool currentIsAttacker, bool showFireControls)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table class='game-grid'>");
            sb.AppendLine("<tr class='head'><td></td>" + string.Join("", Enumerable.Range(0, Board.BoardSize).Select(i => $"<td>{i}</td>")) + "</tr>");

            for (int r = 0; r < Board.BoardSize; r++)
            {
                sb.Append($"<tr><td class='head'>{r}</td>");
                for (int c = 0; c < Board.BoardSize; c++)
                {
                    var cell = defender.Own.Grid[r][c];
                    string cls = cell switch
                    {
                        Cell.Hit => "hit",
                        Cell.Miss => "miss",
                        _ => ""
                    };
                    bool alreadyShot = (cell == Cell.Hit || cell == Cell.Miss);
                    if (showFireControls && currentIsAttacker && !alreadyShot)
                    {
                        sb.Append($"<td class='{cls}'><button class='fire-btn' data-row='{r}' data-col='{c}' title='Bắn vào {r},{c}'>🎯</button></td>");
                    }
                    else
                    {
                        string text = ""; // thay vì "X" / "•"
                        sb.Append($"<td class='{cls}'>{text}</td>");
                    }
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private static string RenderOwn(Board own)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table class='game-grid'>");
            sb.AppendLine("<tr class='head'><td></td>" + string.Join("", Enumerable.Range(0, Board.BoardSize).Select(i => $"<td>{i}</td>")) + "</tr>");
            for (int r = 0; r < Board.BoardSize; r++)
            {
                sb.Append($"<tr><td class='head'>{r}</td>");
                for (int c = 0; c < Board.BoardSize; c++)
                {
                    var cell = own.Grid[r][c];
                    string cls = cell switch
                    {
                        Cell.Hit => "hit",
                        Cell.Miss => "miss",
                        Cell.Ship => GetShipClass(own, r, c),
                        _ => ""
                    };
                    string text = ""; // thay vì "X" / "•" / "S"
                    sb.Append($"<td class='{cls}'>{text}</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private static string GetShipClass(Board board, int r, int c)
        {
            // Handle both Ship and Hit cells
            if (board.Grid[r][c] != Cell.Ship && board.Grid[r][c] != Cell.Hit) return "";
            
            // Determine ship size and direction
            var (shipSize, isVertical) = GetShipSizeAndDirection(board, r, c);
            string directionClass = isVertical ? "vertical" : "horizontal";
            string hitClass = board.Grid[r][c] == Cell.Hit ? " hit" : "";
            
            // Determine position in ship (start, middle, end)
            string positionClass = GetShipPosition(board, r, c, isVertical);
            
            return shipSize switch
            {
                2 => $"ship ship-1 {directionClass} {positionClass}{hitClass}", // 2-cell ship
                3 => $"ship ship-2 {directionClass} {positionClass}{hitClass}", // 3-cell ship
                _ => $"ship {directionClass} {positionClass}{hitClass}" // fallback
            };
        }

        private static string GetShipPosition(Board board, int r, int c, bool isVertical)
        {
            if (isVertical)
            {
                // For vertical ships, check if this is the topmost cell
                bool isTop = r == 0 || (board.Grid[r - 1][c] != Cell.Ship && board.Grid[r - 1][c] != Cell.Hit);
                if (isTop) return "ship-start";
                
                // Check if this is the bottommost cell
                bool isBottom = r == Board.BoardSize - 1 || (board.Grid[r + 1][c] != Cell.Ship && board.Grid[r + 1][c] != Cell.Hit);
                if (isBottom) return "ship-end";
                
                return "ship-middle";
            }
            else
            {
                // For horizontal ships, check if this is the leftmost cell
                bool isLeft = c == 0 || (board.Grid[r][c - 1] != Cell.Ship && board.Grid[r][c - 1] != Cell.Hit);
                if (isLeft) return "ship-start";
                
                // Check if this is the rightmost cell
                bool isRight = c == Board.BoardSize - 1 || (board.Grid[r][c + 1] != Cell.Ship && board.Grid[r][c + 1] != Cell.Hit);
                if (isRight) return "ship-end";
                
                return "ship-middle";
            }
        }

        private static bool IsFirstCellOfShip(Board board, int r, int c)
        {
            // Check if there's a ship cell to the left (horizontal) or above (vertical)
            bool hasLeft = c > 0 && (board.Grid[r][c - 1] == Cell.Ship || board.Grid[r][c - 1] == Cell.Hit);
            bool hasAbove = r > 0 && (board.Grid[r - 1][c] == Cell.Ship || board.Grid[r - 1][c] == Cell.Hit);
            
            // If no ship cells to the left or above, this is the first cell
            return !hasLeft && !hasAbove;
        }

        private static (int size, bool isVertical) GetShipSizeAndDirection(Board board, int r, int c)
        {
            // Check horizontal ship
            int horizontalSize = 1;
            // Check right
            for (int i = c + 1; i < Board.BoardSize && (board.Grid[r][i] == Cell.Ship || board.Grid[r][i] == Cell.Hit); i++)
                horizontalSize++;
            // Check left
            for (int i = c - 1; i >= 0 && (board.Grid[r][i] == Cell.Ship || board.Grid[r][i] == Cell.Hit); i--)
                horizontalSize++;

            // Check vertical ship
            int verticalSize = 1;
            // Check down
            for (int i = r + 1; i < Board.BoardSize && (board.Grid[i][c] == Cell.Ship || board.Grid[i][c] == Cell.Hit); i++)
                verticalSize++;
            // Check up
            for (int i = r - 1; i >= 0 && (board.Grid[i][c] == Cell.Ship || board.Grid[i][c] == Cell.Hit); i--)
                verticalSize++;

            // Return the larger size and its orientation
            if (verticalSize > horizontalSize)
                return (verticalSize, true);
            else
                return (horizontalSize, false);
        }
    }
}
