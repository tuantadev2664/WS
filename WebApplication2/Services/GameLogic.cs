using MiniBattleship.Models;

namespace MiniBattleship.Services
{
    public static class GameLogic
    {

        private static readonly int[] ShipLengths = new[] { 3, 2, 2 };

        public static void PlaceShipsRandomly(Board board, Random rng)
        {
            foreach (var len in ShipLengths)
            {
                bool placed = false;
                int tries = 0;
                while (!placed && tries < 1000)
                {
                    tries++;
                    bool horiz = rng.Next(2) == 0;
                    int r = rng.Next(Board.BoardSize);
                    int c = rng.Next(Board.BoardSize);
                    if (horiz)
                    {
                        if (c + len > Board.BoardSize) continue;
                        bool ok = true;
                        for (int i = 0; i < len; i++) if (board.Grid[r][c + i] != Cell.Empty) { ok = false; break; }
                        if (!ok) continue;
                        for (int i = 0; i < len; i++) board.Grid[r][c + i] = Cell.Ship;
                        placed = true;
                    }
                    else
                    {
                        if (r + len > Board.BoardSize) continue;
                        bool ok = true;
                        for (int i = 0; i < len; i++) if (board.Grid[r + i][c] != Cell.Empty) { ok = false; break; }
                        if (!ok) continue;
                        for (int i = 0; i < len; i++) board.Grid[r + i][c] = Cell.Ship;
                        placed = true;
                    }
                }
            }
        }

        public static (bool hit, bool validShot) Fire(Board defenderBoard, Board attackerOppView, int r, int c)
        {
            if (r < 0 || r >= Board.BoardSize || c < 0 || c >= Board.BoardSize)
                return (false, false);

            // đã bắn ô này rồi?
            if (attackerOppView.Grid[r][c] == Cell.Hit || attackerOppView.Grid[r][c] == Cell.Miss)
                return (false, false);

            if (defenderBoard.Grid[r][c] == Cell.Ship)
            {
                defenderBoard.Grid[r][c] = Cell.Hit;
                attackerOppView.Grid[r][c] = Cell.Hit;
                return (true, true);
            }
            else if (defenderBoard.Grid[r][c] == Cell.Empty)
            {
                defenderBoard.Grid[r][c] = Cell.Miss;
                attackerOppView.Grid[r][c] = Cell.Miss;
                return (false, true);
            }
            else
            {
                return (false, false);
            }
        }


        public static (bool done, string winner) CheckGameOver(GameState g)
        {
            int aRemain = g.A.Own.RemainingShipCells();
            int bRemain = g.B.Own.RemainingShipCells();
            if (aRemain == 0 && bRemain == 0) return (true, "Draw");
            if (aRemain == 0) return (true, g.B.Name);
            if (bRemain == 0) return (true, g.A.Name);
            return (false, string.Empty);
        }


        public static (int r, int c) BotPickShot(Board botOppView, Random rng)
        {
            var candidates = new List<(int r, int c)>();
            for (int r = 0; r < Board.BoardSize; r++)
            {
                for (int c = 0; c < Board.BoardSize; c++)
                {
                    if (botOppView.Grid[r][c] != Cell.Hit && botOppView.Grid[r][c] != Cell.Miss)
                    {
                        candidates.Add((r, c));
                    }
                }
            }
            if (candidates.Count == 0) return (-1, -1);
            return candidates[rng.Next(candidates.Count)];
        }
    }
}
