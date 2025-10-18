namespace MiniBattleship.Models
{
    public class Board
    {
        public const int BoardSize = 7;
        public Cell[][] Grid { get; }


        public Board(Cell[][] grid)
        {
            Grid = grid;
        }


        public static Board CreateEmpty()
        {
            var rows = new Cell[BoardSize][];
            for (int r = 0; r < BoardSize; r++)
                rows[r] = new Cell[BoardSize];
            return new Board(rows);
        }


        public int RemainingShipCells()
        {
            int count = 0;
            for (int r = 0; r < BoardSize; r++)
                for (int c = 0; c < BoardSize; c++)
                    if (Grid[r][c] == Cell.Ship) count++;
            return count;
        }
    }
}
