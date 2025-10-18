using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using MiniBattleship.Models;
using MiniBattleship.Services;
using MiniBattleship.Utils;

namespace WebApplication2.Controllers
{
    public class FireRequest
    {
        public int r { get; set; }
        public int c { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // API Endpoints for AJAX
        [HttpPost]
        public IActionResult Fire([FromBody] FireRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Fire request is null");
                    return Json(new { success = false, message = "Invalid request" });
                }

                _logger.LogInformation($"Fire request: r={request.r}, c={request.c}");
                
                var gameState = SessionJson.Get<GameState>(HttpContext.Session, "game");
                if (gameState == null)
                {
                    _logger.LogWarning("Game not found in session");
                    return Json(new { success = false, message = "Game not found" });
                }

                var current = gameState.Active == Turn.A ? gameState.A : gameState.B;
                var opponent = gameState.Active == Turn.A ? gameState.B : gameState.A;

                _logger.LogInformation($"Current player: {current.Name}, Opponent: {opponent.Name}");
                _logger.LogInformation($"Board size: {Board.BoardSize}, Coordinates: r={request.r}, c={request.c}");

                // Check if coordinates are valid
                if (request.r < 0 || request.r >= Board.BoardSize || request.c < 0 || request.c >= Board.BoardSize)
                {
                    _logger.LogWarning($"Invalid coordinates: r={request.r}, c={request.c}");
                    return Json(new { success = false, message = "Invalid coordinates" });
                }

                // Check if already shot
                if (current.OppView.Grid[request.r][request.c] == Cell.Hit || current.OppView.Grid[request.r][request.c] == Cell.Miss)
                {
                    _logger.LogWarning($"Already shot at r={request.r}, c={request.c}");
                    return Json(new { success = false, message = "Already shot this cell" });
                }

                var (hit, validShot) = GameLogic.Fire(opponent.Own, current.OppView, request.r, request.c);
                _logger.LogInformation($"Fire result: hit={hit}, validShot={validShot}");

                if (!validShot)
                {
                    return Json(new { success = false, message = "Invalid shot" });
                }

                if (gameState.Active == Turn.A)
                    gameState.ShotsTakenA++;
                else
                    gameState.ShotsTakenB++;

                var (done, winner) = GameLogic.CheckGameOver(gameState);
                if (done)
                {
                    gameState.IsGameOver = true;
                    gameState.Winner = winner;
                }
                else
                {
                    // Switch turns
                    gameState.Active = gameState.Active == Turn.A ? Turn.B : Turn.A;
                }

                // Bot auto-fire logic
                var botResult = new { hit = false, botShot = false, botRow = -1, botCol = -1 };
                if (gameState.Mode == Mode.Bot && gameState.Active == Turn.B && !gameState.IsGameOver)
                {
                    botResult = HandleBotTurn(gameState);
                }

                SessionJson.Set(HttpContext.Session, "game", gameState);

                return Json(new { 
                    success = true, 
                    hit = hit, 
                    gameOver = gameState.IsGameOver,
                    winner = gameState.Winner,
                    shotsA = gameState.ShotsTakenA,
                    shotsB = gameState.ShotsTakenB,
                    active = gameState.Active.ToString(),
                    botShot = botResult.botShot,
                    botRow = botResult.botRow,
                    botCol = botResult.botCol,
                    botHit = botResult.hit
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Fire action");
                return Json(new { success = false, message = "Server error" });
            }
        }

        [HttpPost]
        public IActionResult SwitchTurn()
        {
            try
            {
                var gameState = SessionJson.Get<GameState>(HttpContext.Session, "game");
                if (gameState == null)
                {
                    return Json(new { success = false, message = "Game not found" });
                }

                gameState.Active = gameState.Active == Turn.A ? Turn.B : Turn.A;
                SessionJson.Set(HttpContext.Session, "game", gameState);

                return Json(new { success = true, active = gameState.Active.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SwitchTurn action");
                return Json(new { success = false, message = "Server error" });
            }
        }

        [HttpGet]
        public IActionResult GetGameState()
        {
            try
            {
                var gameState = SessionJson.Get<GameState>(HttpContext.Session, "game");
                if (gameState == null)
                {
                    return Json(new { success = false, message = "Game not found" });
                }

                return Json(new { 
                    success = true,
                    gameOver = gameState.IsGameOver,
                    winner = gameState.Winner,
                    shotsA = gameState.ShotsTakenA,
                    shotsB = gameState.ShotsTakenB,
                    active = gameState.Active.ToString(),
                    mode = gameState.Mode.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGameState action");
                return Json(new { success = false, message = "Server error" });
            }
        }

        [HttpGet]
        public IActionResult GetBoards()
        {
            try
            {
                var gameState = SessionJson.Get<GameState>(HttpContext.Session, "game");
                if (gameState == null)
                {
                    return Content("Game not found", "text/html");
                }

                var current = gameState.Active == Turn.A ? gameState.A : gameState.B;
                var opponent = gameState.Active == Turn.A ? gameState.B : gameState.A;

                var html = RenderBoards(gameState, true);
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBoards action");
                return Content("Error loading boards", "text/html");
            }
        }

        [HttpGet]
        public IActionResult DebugGame()
        {
            try
            {
                var gameState = SessionJson.Get<GameState>(HttpContext.Session, "game");
                if (gameState == null)
                {
                    return Json(new { success = false, message = "No game in session" });
                }

                return Json(new { 
                    success = true,
                    hasGame = true,
                    active = gameState.Active.ToString(),
                    mode = gameState.Mode.ToString(),
                    shotsA = gameState.ShotsTakenA,
                    shotsB = gameState.ShotsTakenB,
                    gameOver = gameState.IsGameOver,
                    winner = gameState.Winner
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DebugGame action");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string RenderBoards(GameState g, bool showFireControls)
        {
            var sb = new StringBuilder();
            var current = g.Active == Turn.A ? g.A : g.B;
            var opponent = g.Active == Turn.A ? g.B : g.A;

            sb.AppendLine("<div class='boards-container'>");

            sb.AppendLine($"<div class='board-section'>");
            sb.AppendLine($"<h3 class='board-title'>🎯 Bảng bắn – {System.Net.WebUtility.HtmlEncode(opponent.Name)}</h3>");
            sb.AppendLine(RenderGrid(opponent, currentIsAttacker: true, showFireControls));
            sb.AppendLine("</div>");

            sb.AppendLine($"<div class='board-section'>");
            sb.AppendLine($"<h3 class='board-title'>🚢 Bảng của bạn – {System.Net.WebUtility.HtmlEncode(current.Name)}</h3>");
            sb.AppendLine(RenderOwn(current.Own));
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private string RenderGrid(PlayerState defender, bool currentIsAttacker, bool showFireControls)
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

        private string RenderOwn(Board own)
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

        private string GetShipClass(Board board, int r, int c)
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

        private string GetShipPosition(Board board, int r, int c, bool isVertical)
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

        private bool IsFirstCellOfShip(Board board, int r, int c)
        {
            // Check if there's a ship cell to the left (horizontal) or above (vertical)
            bool hasLeft = c > 0 && (board.Grid[r][c - 1] == Cell.Ship || board.Grid[r][c - 1] == Cell.Hit);
            bool hasAbove = r > 0 && (board.Grid[r - 1][c] == Cell.Ship || board.Grid[r - 1][c] == Cell.Hit);
            
            // If no ship cells to the left or above, this is the first cell
            return !hasLeft && !hasAbove;
        }

        private (int size, bool isVertical) GetShipSizeAndDirection(Board board, int r, int c)
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

        private dynamic HandleBotTurn(GameState gameState)
        {
            try
            {
                _logger.LogInformation("Bot is taking turn...");
                
                var bot = gameState.B;
                var human = gameState.A;
                var rng = new Random();
                
                // Bot picks a shot using AI
                var (botRow, botCol) = GameLogic.BotPickShotAI(bot.OppView, gameState.BotAI, rng);
                
                if (botRow == -1 || botCol == -1)
                {
                    _logger.LogWarning("Bot couldn't find a valid shot");
                    return new { hit = false, botShot = false, botRow = -1, botCol = -1 };
                }
                
                _logger.LogInformation($"Bot firing at ({botRow}, {botCol})");
                
                // Bot fires
                var (botHit, botValid) = GameLogic.Fire(human.Own, bot.OppView, botRow, botCol);
                
                if (botValid)
                {
                    gameState.ShotsTakenB++;
                    _logger.LogInformation($"Bot shot result: hit={botHit}");
                    
                    // Update bot AI state based on shot result
                    GameLogic.UpdateBotAI(gameState.BotAI, bot.OppView, botRow, botCol, botHit);
                }
                
                // Check for game over after bot shot
                var (done, winner) = GameLogic.CheckGameOver(gameState);
                if (done)
                {
                    gameState.IsGameOver = true;
                    gameState.Winner = winner;
                }
                else
                {
                    // Switch back to human
                    gameState.Active = Turn.A;
                }
                
                return new { 
                    hit = botHit, 
                    botShot = true, 
                    botRow = botRow, 
                    botCol = botCol 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bot turn");
                return new { hit = false, botShot = false, botRow = -1, botCol = -1 };
            }
        }
    }
}
