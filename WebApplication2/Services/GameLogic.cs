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
                        
                        // Check if all cells are empty and have proper spacing
                        if (CanPlaceShipHorizontally(board, r, c, len))
                        {
                            // Place the ship
                            for (int i = 0; i < len; i++) 
                                board.Grid[r][c + i] = Cell.Ship;
                            
                            // Validate that no ships are overlapping
                            if (ValidateShipPlacement(board))
                            {
                                placed = true;
                            }
                            else
                            {
                                // Remove the ship if it causes overlap
                                for (int i = 0; i < len; i++) 
                                    board.Grid[r][c + i] = Cell.Empty;
                            }
                        }
                    }
                    else
                    {
                        if (r + len > Board.BoardSize) continue;
                        
                        // Check if all cells are empty and have proper spacing
                        if (CanPlaceShipVertically(board, r, c, len))
                        {
                            // Place the ship
                            for (int i = 0; i < len; i++) 
                                board.Grid[r + i][c] = Cell.Ship;
                            
                            // Validate that no ships are overlapping
                            if (ValidateShipPlacement(board))
                            {
                                placed = true;
                            }
                            else
                            {
                                // Remove the ship if it causes overlap
                                for (int i = 0; i < len; i++) 
                                    board.Grid[r + i][c] = Cell.Empty;
                            }
                        }
                    }
                }
            }
        }

        private static bool CanPlaceShipHorizontally(Board board, int r, int c, int len)
        {
            // Check the ship cells themselves
            for (int i = 0; i < len; i++)
            {
                if (board.Grid[r][c + i] != Cell.Empty) return false;
            }
            
            // Check surrounding cells for proper spacing (1 cell buffer)
            for (int i = -1; i <= len; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int checkR = r + j;
                    int checkC = c + i;
                    
                    // Skip the ship cells themselves
                    if (j == 0 && i >= 0 && i < len) continue;
                    
                    // Check if position is within bounds
                    if (checkR >= 0 && checkR < Board.BoardSize && 
                        checkC >= 0 && checkC < Board.BoardSize)
                    {
                        // If there's already a ship in the buffer zone, can't place
                        if (board.Grid[checkR][checkC] == Cell.Ship) return false;
                    }
                }
            }
            
            return true;
        }

        private static bool CanPlaceShipVertically(Board board, int r, int c, int len)
        {
            // Check the ship cells themselves
            for (int i = 0; i < len; i++)
            {
                if (board.Grid[r + i][c] != Cell.Empty) return false;
            }
            
            // Check surrounding cells for proper spacing (1 cell buffer)
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= len; j++)
                {
                    int checkR = r + j;
                    int checkC = c + i;
                    
                    // Skip the ship cells themselves
                    if (i == 0 && j >= 0 && j < len) continue;
                    
                    // Check if position is within bounds
                    if (checkR >= 0 && checkR < Board.BoardSize && 
                        checkC >= 0 && checkC < Board.BoardSize)
                    {
                        // If there's already a ship in the buffer zone, can't place
                        if (board.Grid[checkR][checkC] == Cell.Ship) return false;
                    }
                }
            }
            
            return true;
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

        // Enhanced bot AI with hunting strategy and proximity targeting
        public static (int r, int c) BotPickShotAI(Board botOppView, BotAIState botAI, Random rng)
        {
            // Priority 1: If bot is hunting, continue hunting
            if (botAI.IsHunting)
            {
                var huntTarget = botAI.GetNextHuntTarget();
                if (huntTarget.HasValue)
                {
                    var (row, col) = huntTarget.Value;
                    // Check if target is valid (not already shot)
                    if (botOppView.Grid[row][col] != Cell.Hit && botOppView.Grid[row][col] != Cell.Miss)
                    {
                        return (row, col);
                    }
                }
                
                // If no valid hunt targets, stop hunting
                botAI.IsHunting = false;
            }
            
            // Priority 2: Proximity targeting - shoot near recent hits
            if (botAI.RecentHits.Count > 0)
            {
                var proximityTargets = botAI.GetProximityTargetsWithPriority();
                
                foreach (var (row, col, priority) in proximityTargets)
                {
                    // Check if target is valid (not already shot)
                    if (botOppView.Grid[row][col] != Cell.Hit && botOppView.Grid[row][col] != Cell.Miss)
                    {
                        return (row, col);
                    }
                }
            }
            
            // Priority 3: If not hunting and no proximity targets, use parity targeting
            if (botAI.IsParityMode)
            {
                // Generate parity targets if not already done
                if (botAI.ParityTargets.Count == 0)
                {
                    botAI.GenerateParityTargets();
                }
                
                // Try parity targets
                var parityTarget = botAI.GetNextParityTarget();
                if (parityTarget.HasValue)
                {
                    var (row, col) = parityTarget.Value;
                    // Check if target is valid
                    if (botOppView.Grid[row][col] != Cell.Hit && botOppView.Grid[row][col] != Cell.Miss)
                    {
                        return (row, col);
                    }
                }
                
                // If no valid parity targets, switch to random
                botAI.IsParityMode = false;
            }
            
            // Priority 4: Fallback to random targeting
            return BotPickShot(botOppView, rng);
        }

        // Update bot AI state after a shot
        public static void UpdateBotAI(BotAIState botAI, Board botOppView, int row, int col, bool hit)
        {
            if (hit)
            {
                // Add to recent hits for proximity targeting
                botAI.AddRecentHit(row, col);
                
                if (!botAI.IsHunting)
                {
                    // Start hunting mode
                    botAI.StartHunting(row, col);
                }
                else
                {
                    // Continue hunting - determine direction if unknown
                    if (botAI.HuntDirection == "unknown")
                    {
                        // Check if this hit is adjacent to the first hit
                        if (Math.Abs(row - botAI.HuntStartRow) == 1 && col == botAI.HuntStartCol)
                        {
                            botAI.ContinueHunting("vertical");
                        }
                        else if (Math.Abs(col - botAI.HuntStartCol) == 1 && row == botAI.HuntStartRow)
                        {
                            botAI.ContinueHunting("horizontal");
                        }
                    }
                }
            }
            else
            {
                // If hunting and missed, try other direction or stop hunting
                if (botAI.IsHunting && botAI.HuntDirection == "unknown")
                {
                    // Try the other direction
                    if (botAI.HuntIndex >= botAI.HuntTargets.Count)
                    {
                        // No more targets in current direction, try other direction
                        botAI.HuntDirection = "horizontal"; // Try horizontal if was trying vertical
                        botAI.ContinueHunting("horizontal");
                    }
                }
                else if (botAI.IsHunting && botAI.HuntIndex >= botAI.HuntTargets.Count)
                {
                    // No more targets, stop hunting
                    botAI.IsHunting = false;
                }
            }
        }

        // Method to validate that no ships are overlapping
        public static bool ValidateShipPlacement(Board board)
        {
            // Check for any overlapping ships by looking for adjacent ship cells
            for (int r = 0; r < Board.BoardSize; r++)
            {
                for (int c = 0; c < Board.BoardSize; c++)
                {
                    if (board.Grid[r][c] == Cell.Ship)
                    {
                        // Check diagonal cells for other ships (these should never be adjacent to the same ship)
                        if (HasDiagonalShipNeighbor(board, r, c))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static bool HasDiagonalShipNeighbor(Board board, int r, int c)
        {
            // Check the 4 diagonal positions
            int[] diagonalRows = { -1, -1, 1, 1 };
            int[] diagonalCols = { -1, 1, -1, 1 };
            
            for (int i = 0; i < 4; i++)
            {
                int checkR = r + diagonalRows[i];
                int checkC = c + diagonalCols[i];
                
                if (checkR >= 0 && checkR < Board.BoardSize && 
                    checkC >= 0 && checkC < Board.BoardSize)
                {
                    if (board.Grid[checkR][checkC] == Cell.Ship)
                    {
                        // Found a diagonal ship cell, which means ships are too close
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
