using System;
using System.Collections.Generic;
using System.Linq;

namespace ChainReaction.Model
{
    public class BotEngine
    {
        private static readonly Random _rng = new();

        /// <summary>
        /// Analyzes the board state and returns the optimal cell to place an orb for the given bot player.
        /// </summary>
        public static Cell? FindBestMove(
            List<List<Cell>> grid,
            int rows,
            int cols,
            Player botPlayer,
            List<Player> allPlayers,
            string difficulty)
        {
            var validCells = new List<Cell>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var cell = grid[r][c];
                    if (cell.IsAvailableFor(botPlayer.Name))
                    {
                        validCells.Add(cell);
                    }
                }
            }

            if (validCells.Count == 0) return null;

            var normalizedDifficulty = difficulty?.ToLowerInvariant() switch
            {
                "easy" => "easy",
                "hard" => "hard",
                _ => "medium"
            };

            // Easy mode: 35% chance to make an uncalculated casual move
            if (normalizedDifficulty == "easy" && _rng.NextDouble() < 0.35)
            {
                return validCells[_rng.Next(validCells.Count)];
            }

            var scoredMoves = new List<(Cell Cell, double Score)>();

            foreach (var cell in validCells)
            {
                double score = EvaluateMove(grid, rows, cols, cell, botPlayer, allPlayers, normalizedDifficulty);
                scoredMoves.Add((cell, score));
            }

            // Order moves by highest score
            var ordered = scoredMoves.OrderByDescending(m => m.Score).ToList();

            if (normalizedDifficulty == "easy")
            {
                // Pick among top 3 moves randomly
                int topK = Math.Min(3, ordered.Count);
                return ordered[_rng.Next(topK)].Cell;
            }
            else if (normalizedDifficulty == "medium")
            {
                // If top scores are very close (within 5 points), randomize between them for natural gameplay
                double maxScore = ordered[0].Score;
                var topCandidates = ordered.Where(m => m.Score >= maxScore - 4.0).ToList();
                return topCandidates[_rng.Next(topCandidates.Count)].Cell;
            }
            else // Hard / Master
            {
                // Pick highest score (randomize only on exact ties)
                double maxScore = ordered[0].Score;
                var topCandidates = ordered.Where(m => Math.Abs(m.Score - maxScore) < 0.001).ToList();
                return topCandidates[_rng.Next(topCandidates.Count)].Cell;
            }
        }

        private static double EvaluateMove(
            List<List<Cell>> grid,
            int rows,
            int cols,
            Cell candidate,
            Player botPlayer,
            List<Player> allPlayers,
            string difficulty)
        {
            double score = 0.0;
            int r = candidate.X;
            int c = candidate.Y;
            int capacity = candidate.Capacity;
            int currentCount = candidate.CurrentCount;
            bool willExplode = (currentCount + 1) > capacity;

            // 1. Positional Value (Corner > Edge > Center in early/mid game)
            if (capacity == 1) // Corner (2 neighbors)
            {
                score += 30.0;
            }
            else if (capacity == 2) // Edge (3 neighbors)
            {
                score += 18.0;
            }
            else // Center (4 neighbors)
            {
                score += 8.0;
            }

            // 2. Danger Zone Analysis (Immediate enemy threat from adjacent cells)
            var neighbors = GetNeighborCoords(r, c, rows, cols);
            int adjacentEnemyCriticalCount = 0;
            int adjacentEnemyOrbCount = 0;

            foreach (var (nr, nc) in neighbors)
            {
                var nCell = grid[nr][nc];
                if (!string.IsNullOrEmpty(nCell.Name) && nCell.Name != botPlayer.Name)
                {
                    adjacentEnemyOrbCount += nCell.CurrentCount;
                    if (nCell.IsCritical) // Enemy cell is 1 orb away from exploding!
                    {
                        adjacentEnemyCriticalCount++;
                    }
                }
            }

            if (!willExplode)
            {
                // If we don't explode this turn and put an orb adjacent to an enemy critical cell,
                // the enemy can detonate on their next turn and take our cell!
                if (adjacentEnemyCriticalCount > 0)
                {
                    if (difficulty == "hard")
                        score -= 80.0 * adjacentEnemyCriticalCount;
                    else if (difficulty == "medium")
                        score -= 50.0 * adjacentEnemyCriticalCount;
                    else
                        score -= 20.0 * adjacentEnemyCriticalCount;
                }

                // If this move makes our cell critical and no adjacent enemy is critical,
                // we create an offensive threat against adjacent enemy or empty cells!
                if (currentCount + 1 == capacity)
                {
                    score += 22.0;
                    if (adjacentEnemyOrbCount > 0)
                    {
                        score += 15.0; // Threatening enemy orbs
                    }
                }
                else
                {
                    // Adding orbs to our non-critical cell builds presence
                    score += (currentCount + 1) * 4.0;
                }
            }
            else
            {
                // 3. Explosion & Simulation Analysis
                var simResult = SimulateMove(grid, rows, cols, r, c, botPlayer.Name);

                if (simResult.IsVictory)
                {
                    return 100000.0; // Instant winning move!
                }

                score += simResult.EliminatedPlayers * 300.0;
                score += simResult.EnemyOrbsCaptured * 20.0;
                score += simResult.EnemyCellsCaptured * 25.0;
                score += simResult.NetOrbsGained * 6.0;
                score += simResult.ChainExplosionSteps * 8.0;

                // If our explosion destroyed an adjacent enemy critical cell, bonus for neutralizing threat
                if (adjacentEnemyCriticalCount > 0)
                {
                    score += 60.0 * adjacentEnemyCriticalCount;
                }

                // In Hard difficulty, check post-explosion safety:
                if (difficulty == "hard")
                {
                    int vulnerablePostCells = CountVulnerableCells(simResult.Counts, simResult.Owners, rows, cols, botPlayer.Name);
                    score -= vulnerablePostCells * 15.0;
                }
            }

            // Add small controlled noise to avoid robotic predictability
            if (difficulty == "easy")
            {
                score += (_rng.NextDouble() * 30.0) - 15.0;
            }
            else if (difficulty == "medium")
            {
                score += (_rng.NextDouble() * 8.0) - 4.0;
            }
            else // Hard
            {
                score += (_rng.NextDouble() * 2.0) - 1.0;
            }

            return score;
        }

        private static List<(int X, int Y)> GetNeighborCoords(int r, int c, int rows, int cols)
        {
            var list = new List<(int X, int Y)>(4);
            if (r > 0) list.Add((r - 1, c));
            if (r < rows - 1) list.Add((r + 1, c));
            if (c > 0) list.Add((r, c - 1));
            if (c < cols - 1) list.Add((r, c + 1));
            return list;
        }

        private static int GetCapacity(int r, int c, int rows, int cols)
        {
            bool isCornerR = (r == 0 || r == rows - 1);
            bool isCornerC = (c == 0 || c == cols - 1);
            if (isCornerR && isCornerC) return 1;
            if (isCornerR || isCornerC) return 2;
            return 3;
        }

        private class SimulationResult
        {
            public int EnemyOrbsCaptured { get; set; }
            public int EnemyCellsCaptured { get; set; }
            public int NetOrbsGained { get; set; }
            public int ChainExplosionSteps { get; set; }
            public int EliminatedPlayers { get; set; }
            public bool IsVictory { get; set; }
            public int[,] Counts { get; set; } = null!;
            public string[,] Owners { get; set; } = null!;
        }

        private static SimulationResult SimulateMove(
            List<List<Cell>> grid,
            int rows,
            int cols,
            int startR,
            int startC,
            string botName)
        {
            int[,] counts = new int[rows, cols];
            string[,] owners = new string[rows, cols];
            int initialBotOrbs = 0;
            int initialEnemyOrbs = 0;
            var initialAlivePlayers = new HashSet<string>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var cell = grid[i][j];
                    counts[i, j] = cell.CurrentCount;
                    owners[i, j] = cell.Name;
                    if (!string.IsNullOrEmpty(cell.Name) && cell.CurrentCount > 0)
                    {
                        initialAlivePlayers.Add(cell.Name);
                        if (cell.Name == botName)
                            initialBotOrbs += cell.CurrentCount;
                        else
                            initialEnemyOrbs += cell.CurrentCount;
                    }
                }
            }

            int enemyOrbsCaptured = 0;
            int enemyCellsCaptured = 0;
            int chainSteps = 0;

            // Apply candidate move
            counts[startR, startC]++;
            owners[startR, startC] = botName;

            Queue<(int X, int Y)> queue = new();
            if (counts[startR, startC] > GetCapacity(startR, startC, rows, cols))
            {
                queue.Enqueue((startR, startC));
            }

            int maxSteps = 150; // Safety guard against infinite loops
            while (queue.Count > 0 && chainSteps < maxSteps)
            {
                chainSteps++;
                int waveSize = queue.Count;
                List<(int X, int Y)> nextExplosions = new();

                for (int w = 0; w < waveSize; w++)
                {
                    var (cx, cy) = queue.Dequeue();
                    int cap = GetCapacity(cx, cy, rows, cols);
                    if (counts[cx, cy] <= cap) continue;

                    counts[cx, cy] = 0;
                    owners[cx, cy] = string.Empty;

                    var nbrs = GetNeighborCoords(cx, cy, rows, cols);
                    foreach (var (nx, ny) in nbrs)
                    {
                        var prevOwner = owners[nx, ny];
                        int prevCount = counts[nx, ny];

                        if (!string.IsNullOrEmpty(prevOwner) && prevOwner != botName && prevCount > 0)
                        {
                            enemyOrbsCaptured += prevCount;
                            enemyCellsCaptured++;
                        }

                        counts[nx, ny]++;
                        owners[nx, ny] = botName;

                        if (counts[nx, ny] > GetCapacity(nx, ny, rows, cols) && !nextExplosions.Contains((nx, ny)))
                        {
                            nextExplosions.Add((nx, ny));
                        }
                    }
                }

                foreach (var next in nextExplosions)
                {
                    queue.Enqueue(next);
                }
            }

            // Post simulation tally
            int finalBotOrbs = 0;
            var finalAlivePlayers = new HashSet<string>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (!string.IsNullOrEmpty(owners[i, j]) && counts[i, j] > 0)
                    {
                        finalAlivePlayers.Add(owners[i, j]);
                        if (owners[i, j] == botName)
                            finalBotOrbs += counts[i, j];
                    }
                }
            }

            int eliminated = initialAlivePlayers.Count - finalAlivePlayers.Count;
            bool isVictory = (initialAlivePlayers.Count > 1 && finalAlivePlayers.Count == 1 && finalAlivePlayers.Contains(botName));

            return new SimulationResult
            {
                EnemyOrbsCaptured = enemyOrbsCaptured,
                EnemyCellsCaptured = enemyCellsCaptured,
                NetOrbsGained = finalBotOrbs - initialBotOrbs,
                ChainExplosionSteps = chainSteps,
                EliminatedPlayers = Math.Max(0, eliminated),
                IsVictory = isVictory,
                Counts = counts,
                Owners = owners
            };
        }

        private static int CountVulnerableCells(int[,] counts, string[,] owners, int rows, int cols, string botName)
        {
            int vulnerable = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (owners[r, c] == botName && counts[r, c] > 0)
                    {
                        var nbrs = GetNeighborCoords(r, c, rows, cols);
                        foreach (var (nr, nc) in nbrs)
                        {
                            if (!string.IsNullOrEmpty(owners[nr, nc]) && owners[nr, nc] != botName)
                            {
                                int enemyCap = GetCapacity(nr, nc, rows, cols);
                                if (counts[nr, nc] == enemyCap) // Enemy neighbor is critical!
                                {
                                    vulnerable++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return vulnerable;
        }
    }
}
