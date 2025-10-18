using Microsoft.AspNetCore.Http;
using MiniBattleship.Models;
using MiniBattleship.Services;
using MiniBattleship.Utils;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(4);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();
            app.UseSession();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapGet("/", (HttpContext ctx) =>
            {
                var g = ctx.Session.Get<GameState>("game");
                if (g is null)
                {
                    return Results.Redirect("/new?mode=bot");
                }
                return Results.Content(MiniBattleship.Services.HtmlRenderer.HtmlPage(g), "text/html");
            });

            app.MapGet("/new", (HttpContext ctx) =>
            {
                var modeStr = ctx.Request.Query["mode"].ToString();
                var mode = modeStr?.ToLowerInvariant() == "hotseat" ? Mode.HotSeat : Mode.Bot;
                var rng = new Random();


                var g = new GameState
                {
                    Mode = mode,
                    Active = Turn.A,
                    A = new PlayerState(Board.CreateEmpty(), Board.CreateEmpty(), mode == Mode.Bot ? "You" : "Player A"),
                    B = new PlayerState(Board.CreateEmpty(), Board.CreateEmpty(), mode == Mode.Bot ? "Bot" : "Player B"),
                    ShotsTakenA = 0,
                    ShotsTakenB = 0,
                    IsGameOver = false,
                    Winner = string.Empty,
                    BotAI = new BotAIState() // Initialize bot AI
                };


                GameLogic.PlaceShipsRandomly(g.A.Own, rng);
                GameLogic.PlaceShipsRandomly(g.B.Own, rng);


                ctx.Session.Set("game", g);
                return Results.Redirect("/");
            });

            app.MapPost("/fire", (HttpContext ctx) =>
            {
                var g = ctx.Session.Get<GameState>("game");
                if (g is null || g.IsGameOver) return Results.Redirect("/");


                if (!int.TryParse(ctx.Request.Query["r"], out var r)) r = -1;
                if (!int.TryParse(ctx.Request.Query["c"], out var c)) c = -1;


                string flash = string.Empty;
                var rng = new Random();


                if (g.Mode == Mode.Bot)
                {
                    var (hit, valid) = GameLogic.Fire(g.B.Own, g.A.OppView, r, c);
                    if (!valid)
                    {
                        flash = "Ô này đã bắn rồi hoặc không hợp lệ.";
                    }
                    else
                    {
                        g.ShotsTakenA++;
                        flash = hit ? "🎯 HIT!" : "💨 MISS.";
                        var (done, winner) = GameLogic.CheckGameOver(g);
                        if (done)
                        {
                            g.IsGameOver = true; g.Winner = winner;
                            ctx.Session.Set("game", g);
                            return Results.Content(MiniBattleship.Services.HtmlRenderer.HtmlPage(g, flash), "text/html");
                        }
                        var pick = GameLogic.BotPickShot(g.B.OppView, rng);
                        if (pick != (-1, -1))
                        {
                            var (botHit, _) = GameLogic.Fire(g.A.Own, g.B.OppView, pick.r, pick.c);
                            g.ShotsTakenB++;
                            var (done2, winner2) = GameLogic.CheckGameOver(g);
                            if (done2) { g.IsGameOver = true; g.Winner = winner2; }
                            flash += botHit ? " | 🤖 Bot: HIT!" : " | 🤖 Bot: miss.";
                        }
                    }
                }
                else
                {
                    if (g.Active == Turn.A)
                    {
                        var (hit, valid) = GameLogic.Fire(g.B.Own, g.A.OppView, r, c);
                        if (valid) { g.ShotsTakenA++; g.Active = Turn.B; flash = hit ? "🎯 A HIT!" : "💨 A miss."; } else flash = "Ô này đã bắn rồi hoặc không hợp lệ.";
                    }
                    else
                    {
                        var (hit, valid) = GameLogic.Fire(g.A.Own, g.B.OppView, r, c);
                        if (valid) { g.ShotsTakenB++; g.Active = Turn.A; flash = hit ? "🎯 B HIT!" : "💨 B miss."; } else flash = "Ô này đã bắn rồi hoặc không hợp lệ.";
                    }
                    var (done, winner) = GameLogic.CheckGameOver(g);
                    if (done) { g.IsGameOver = true; g.Winner = winner; }
                }


                ctx.Session.Set("game", g);
                return Results.Content(MiniBattleship.Services.HtmlRenderer.HtmlPage(g, flash), "text/html");
            });

            app.MapPost("/switch", (HttpContext ctx) =>
            {
                var g = ctx.Session.Get<GameState>("game");
                if (g is null) return Results.Redirect("/");
                if (g.Mode == Mode.HotSeat)
                {
                    g.Active = g.Active == Turn.A ? Turn.B : Turn.A;
                    ctx.Session.Set("game", g);
                    return Results.Content(HtmlRenderer.HtmlPage(g, "Đã đổi lượt."), "text/html");
                }
                return Results.Redirect("/");
            });

            app.Run();
        }
    }
}
