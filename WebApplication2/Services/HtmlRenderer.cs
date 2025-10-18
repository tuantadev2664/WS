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
            sb.AppendLine("<style>body{font-family:system-ui,Segoe UI,Roboto,Arial;margin:20px}\r\n.wrap{display:grid;grid-template-columns:1fr 1fr;gap:24px}\r\ntable{border-collapse:collapse}\r\ntd{\r\n  width:36px;height:36px;text-align:center;position:relative;\r\n  border:1px solid rgba(255,255,255,0.15);\r\n  background:\r\n    linear-gradient(0deg, rgba(255,255,255,0.06) 1px, transparent 1px) 0 0/36px 36px,\r\n    linear-gradient(90deg, rgba(255,255,255,0.06) 1px, transparent 1px) 0 0/36px 36px,\r\n    linear-gradient(180deg,#0aa1d6,#0b79d0); /* ocean */\r\n  color:#fff\r\n}\r\n.head td, td.head{\r\n  font-weight:700;background:#f4f4f4;color:#222;border-color:#aaa\r\n}\r\n\r\n/* Ship tile (ô có tàu trên bảng của mình) */\r\n.ship{\r\n  background:\r\n    radial-gradient(ellipse at center, rgba(255,255,255,0.25), rgba(255,255,255,0) 60%) center/100% 100% no-repeat,\r\n    linear-gradient(180deg,#8e8e93,#6b6b70);\r\n  box-shadow: inset 0 0 0 2px rgba(255,255,255,0.15);\r\n}\r\n\r\n/* Miss: chấm tròn trắng */\r\n.miss::after{\r\n  content:\"\";\r\n  position:absolute; inset:0;\r\n  margin:auto; width:10px; height:10px; border-radius:50%;\r\n  background:#fff; opacity:.9;\r\n}\r\n\r\n/* Hit: chấm tròn đỏ tươi */\r\n.hit::after{\r\n  content:\"\";\r\n  position:absolute; inset:0;\r\n  margin:auto; width:14px; height:14px; border-radius:50%;\r\n  background:#e53935; box-shadow:0 0 6px rgba(229,57,53,.7);\r\n}\r\n\r\n.btn{padding:8px 12px;border:1px solid #222;background:#222;color:#fff;border-radius:8px;text-decoration:none}\r\n.btn.secondary{background:#fff;color:#222}\r\n.stack{display:flex;gap:8px;flex-wrap:wrap}\r\n.flash{margin:10px 0;padding:10px;border-radius:6px;background:#fff3cd;border:1px solid #ffeeba;color:#6b5900}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<h1>Mini Battleship</h1>");


            if (!string.IsNullOrEmpty(flash)) sb.AppendLine($"<div class='flash'>{WebUtility.HtmlEncode(flash)}</div>");


            if (g.IsGameOver)
            {
                sb.AppendLine($"<h2>Winner: {WebUtility.HtmlEncode(g.Winner)}</h2>");
                sb.AppendLine($"<p>Shots → {WebUtility.HtmlEncode(g.A.Name)}: {g.ShotsTakenA} | {WebUtility.HtmlEncode(g.B.Name)}: {g.ShotsTakenB}</p>");
                sb.AppendLine("<div class='stack'>");
                sb.AppendLine("<a class='btn' href='/new?mode=bot'>Play Again (vs Bot)</a>");
                sb.AppendLine("<a class='btn secondary' href='/new?mode=hotseat'>Play Again (Hot-Seat)</a>");
                sb.AppendLine("</div>");
                sb.AppendLine(RenderBoards(g, showFireControls: false));
                sb.AppendLine("</body></html>");
                return sb.ToString();
            }


            sb.AppendLine($"<p>Mode: <b>{g.Mode}</b> · Turn: <b>{g.Active}</b></p>");
            sb.AppendLine($"<p>{WebUtility.HtmlEncode(g.A.Name)} Shots: {g.ShotsTakenA} | {WebUtility.HtmlEncode(g.B.Name)} Shots: {g.ShotsTakenB}</p>");


            sb.AppendLine("<div class='stack'>");
            sb.AppendLine("<a class='btn' href='/new?mode=bot'>New Game (vs Bot)</a>");
            sb.AppendLine("<a class='btn secondary' href='/new?mode=hotseat'>New Game (Hot-Seat)</a>");
            sb.AppendLine("</div>");


            bool showFire = true;
            sb.AppendLine(RenderBoards(g, showFire));


            if (g.Mode == Mode.HotSeat)
            {
                sb.AppendLine("<form method='post' action='/switch'><button class='btn secondary' type='submit'>Đổi lượt</button></form>");
            }


            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string RenderBoards(GameState g, bool showFireControls)
        {
            var sb = new StringBuilder();
            var current = g.Active == Turn.A ? g.A : g.B;
            var opponent = g.Active == Turn.A ? g.B : g.A;


            sb.AppendLine("<div class='wrap'>");


            sb.AppendLine($"<div><h3>Target Grid – {WebUtility.HtmlEncode(opponent.Name)}</h3>");
            sb.AppendLine(RenderGrid(opponent, currentIsAttacker: true, showFireControls));
            sb.AppendLine("</div>");


            sb.AppendLine($"<div><h3>Your Board – {WebUtility.HtmlEncode(current.Name)}</h3>");
            sb.AppendLine(RenderOwn(current.Own));
            sb.AppendLine("</div>");


            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private static string RenderGrid(PlayerState defender, bool currentIsAttacker, bool showFireControls)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<form method='post' action='/fire'><table>");
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
                        sb.Append($"<td class='{cls}'><button name='r' value='{r}' formaction='/fire?r={r}&c={c}' formmethod='post' style='width:100%;height:100%;border:none;background:transparent;cursor:pointer' title='Fire at {r},{c}'>·</button></td>");
                    }
                    else
                    {
                        string text = ""; // thay vì "X" / "•"
                        sb.Append($"<td class='{cls}'>{text}</td>");
                    }
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table></form>");
            return sb.ToString();
        }

        private static string RenderOwn(Board own)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table>");
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
                        Cell.Ship => "ship",
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
    }
}
