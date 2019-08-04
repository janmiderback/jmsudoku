using System;
using System.Collections.Generic;

namespace SudoEng
{
    /// <summary>
    /// Sudoku problem solver.
    /// </summary>
    /// <remarks>
    /// Solves a Sudoku problem using a backtracking brute-force method.
    /// The solver can be set to terminate when the first solution is found or try to find
    /// all solutions which also provides answer to whether the solution is unique.
    /// </remarks>
    internal sealed class Solver
    {
        private int searchCallCount;
        
        public void Solve(Grid grid, SolverContext solverContext, bool exitAtFirst = false, TimeSpan? maxTime = null)
        {
            if (grid == null) { throw new ArgumentNullException("grid"); }
            if (solverContext == null) { throw new ArgumentNullException("context"); }

            solverContext.StartTime = DateTime.Now;
            searchCallCount = 0;
            solverContext.Problem = grid;

            Grid reducedGrid = grid.Reduce();

            if (reducedGrid != null)
            {
                Search(reducedGrid, solverContext, exitAtFirst, maxTime);
            }

            solverContext.SearchCallCount = searchCallCount;
            solverContext.EndTime = DateTime.Now;
        }

        private void Search(Grid grid, SolverContext solverContext, bool exitAtFirst = false, TimeSpan? maxTime = null)
        {
            searchCallCount++;

            if (grid.IsSolution())
            {
                solverContext.AddSolution(grid);
            }
            else
            {
                IEnumerable<Grid> moves = grid.GenerateMoves();

                if (moves != null)
                {
                    foreach (var move in moves)
                    {
                        Search(move, solverContext, exitAtFirst, maxTime);

                        if ((exitAtFirst && solverContext.UniqueSolution != null) || HasMaxTimeExceeded(solverContext, maxTime))
                        {
                            break;
                        }
                    }
                }
            }
        }

        private bool HasMaxTimeExceeded(SolverContext solverContext, TimeSpan? maxTime)
        {
            if (maxTime != null)
            {
                return DateTime.Now.Subtract(solverContext.StartTime) > maxTime;
            }

            return false;
        }
    }
}
