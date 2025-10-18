namespace MiniBattleship.Models
{
    public class BotAIState
    {
        // Bot hunting mode states
        public bool IsHunting { get; set; } = false;
        public int HuntStartRow { get; set; } = -1;
        public int HuntStartCol { get; set; } = -1;
        public string HuntDirection { get; set; } = ""; // "horizontal", "vertical", "unknown"
        public List<(int row, int col)> HuntTargets { get; set; } = new();
        public int HuntIndex { get; set; } = 0;
        
        // Recent hits for proximity targeting
        public List<(int row, int col)> RecentHits { get; set; } = new();
        public int MaxRecentHits { get; set; } = 5; // Keep track of last 5 hits
        
        // Bot strategy states
        public bool IsParityMode { get; set; } = true; // Start with parity targeting
        public List<(int row, int col)> ParityTargets { get; set; } = new();
        public int ParityIndex { get; set; } = 0;
        
        // Reset bot AI state
        public void Reset()
        {
            IsHunting = false;
            HuntStartRow = -1;
            HuntStartCol = -1;
            HuntDirection = "";
            HuntTargets.Clear();
            HuntIndex = 0;
            IsParityMode = true;
            ParityTargets.Clear();
            ParityIndex = 0;
            RecentHits.Clear();
        }
        
        // Add a recent hit for proximity targeting
        public void AddRecentHit(int row, int col)
        {
            RecentHits.Add((row, col));
            
            // Keep only the most recent hits
            if (RecentHits.Count > MaxRecentHits)
            {
                RecentHits.RemoveAt(0);
            }
        }
        
        // Get proximity targets near recent hits
        public List<(int row, int col)> GetProximityTargets()
        {
            var proximityTargets = new List<(int row, int col)>();
            
            foreach (var (hitRow, hitCol) in RecentHits)
            {
                // Add adjacent cells (up, down, left, right)
                var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
                
                foreach (var (dr, dc) in directions)
                {
                    int newRow = hitRow + dr;
                    int newCol = hitCol + dc;
                    
                    // Check bounds
                    if (newRow >= 0 && newRow < Board.BoardSize && 
                        newCol >= 0 && newCol < Board.BoardSize)
                    {
                        proximityTargets.Add((newRow, newCol));
                    }
                }
            }
            
            // Remove duplicates
            return proximityTargets.Distinct().ToList();
        }
        
        // Get proximity targets with priority (closer to recent hits = higher priority)
        public List<(int row, int col, int priority)> GetProximityTargetsWithPriority()
        {
            var targetsWithPriority = new List<(int row, int col, int priority)>();
            
            foreach (var (hitRow, hitCol) in RecentHits)
            {
                // Add adjacent cells with high priority
                var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
                
                foreach (var (dr, dc) in directions)
                {
                    int newRow = hitRow + dr;
                    int newCol = hitCol + dc;
                    
                    // Check bounds
                    if (newRow >= 0 && newRow < Board.BoardSize && 
                        newCol >= 0 && newCol < Board.BoardSize)
                    {
                        targetsWithPriority.Add((newRow, newCol, 10)); // High priority for adjacent
                    }
                }
                
                // Add diagonal cells with medium priority
                var diagonalDirections = new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) };
                
                foreach (var (dr, dc) in diagonalDirections)
                {
                    int newRow = hitRow + dr;
                    int newCol = hitCol + dc;
                    
                    // Check bounds
                    if (newRow >= 0 && newRow < Board.BoardSize && 
                        newCol >= 0 && newCol < Board.BoardSize)
                    {
                        targetsWithPriority.Add((newRow, newCol, 5)); // Medium priority for diagonal
                    }
                }
            }
            
            // Remove duplicates and sort by priority
            return targetsWithPriority
                .GroupBy(t => (t.row, t.col))
                .Select(g => g.OrderByDescending(t => t.priority).First())
                .OrderByDescending(t => t.priority)
                .ToList();
        }
        
        // Start hunting mode when bot hits a ship
        public void StartHunting(int row, int col)
        {
            IsHunting = true;
            HuntStartRow = row;
            HuntStartCol = col;
            HuntDirection = "unknown";
            HuntTargets.Clear();
            HuntIndex = 0;
            
            // Generate hunting targets around the hit
            GenerateHuntTargets(row, col);
        }
        
        // Generate targets around a hit for hunting
        private void GenerateHuntTargets(int row, int col)
        {
            HuntTargets.Clear();
            
            // Add adjacent cells (up, down, left, right)
            var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            
            foreach (var (dr, dc) in directions)
            {
                int newRow = row + dr;
                int newCol = col + dc;
                
                // Check bounds
                if (newRow >= 0 && newRow < Board.BoardSize && 
                    newCol >= 0 && newCol < Board.BoardSize)
                {
                    HuntTargets.Add((newRow, newCol));
                }
            }
        }
        
        // Continue hunting in a specific direction
        public void ContinueHunting(string direction)
        {
            HuntDirection = direction;
            
            // Clear old targets and generate new ones in the direction
            HuntTargets.Clear();
            
            int dr = 0, dc = 0;
            if (direction == "horizontal")
            {
                dc = 1; // Right
            }
            else if (direction == "vertical")
            {
                dr = 1; // Down
            }
            
            // Add targets in the direction
            for (int i = 1; i <= 3; i++) // Check up to 3 cells in direction
            {
                int newRow = HuntStartRow + (dr * i);
                int newCol = HuntStartCol + (dc * i);
                
                if (newRow >= 0 && newRow < Board.BoardSize && 
                    newCol >= 0 && newCol < Board.BoardSize)
                {
                    HuntTargets.Add((newRow, newCol));
                }
            }
            
            // Also add targets in opposite direction
            for (int i = 1; i <= 3; i++)
            {
                int newRow = HuntStartRow - (dr * i);
                int newCol = HuntStartCol - (dc * i);
                
                if (newRow >= 0 && newRow < Board.BoardSize && 
                    newCol >= 0 && newCol < Board.BoardSize)
                {
                    HuntTargets.Add((newRow, newCol));
                }
            }
            
            HuntIndex = 0;
        }
        
        // Get next hunting target
        public (int row, int col)? GetNextHuntTarget()
        {
            if (!IsHunting || HuntIndex >= HuntTargets.Count)
                return null;
                
            var target = HuntTargets[HuntIndex];
            HuntIndex++;
            return target;
        }
        
        // Generate parity targets (checkerboard pattern)
        public void GenerateParityTargets()
        {
            ParityTargets.Clear();
            
            for (int r = 0; r < Board.BoardSize; r++)
            {
                for (int c = 0; c < Board.BoardSize; c++)
                {
                    // Checkerboard pattern: (r + c) % 2 == 0
                    if ((r + c) % 2 == 0)
                    {
                        ParityTargets.Add((r, c));
                    }
                }
            }
            
            ParityIndex = 0;
        }
        
        // Get next parity target
        public (int row, int col)? GetNextParityTarget()
        {
            if (ParityIndex >= ParityTargets.Count)
                return null;
                
            var target = ParityTargets[ParityIndex];
            ParityIndex++;
            return target;
        }
    }
}
