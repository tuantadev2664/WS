namespace MiniBattleship.Models
{
    public class GameState
    {
        public Mode Mode { get; set; } = Mode.Bot;
        public Turn Active { get; set; } = Turn.A; // In Bot mode, A = Human, B = Bot
        public PlayerState A { get; set; } = new(PlayerStateDefault(), PlayerStateDefault(), "Player A");
        public PlayerState B { get; set; } = new(PlayerStateDefault(), PlayerStateDefault(), "Player B / Bot");
        public int ShotsTakenA { get; set; }
        public int ShotsTakenB { get; set; }
        public bool IsGameOver { get; set; }
        public string Winner { get; set; } = string.Empty;


        private static Board PlayerStateDefault() => Board.CreateEmpty();
    }
}
