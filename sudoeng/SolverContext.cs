using System;
using System.Collections.Generic;

namespace SudoEng
{
    /// <summary>
    /// Solver context providing information on the solver run.
    /// </summary>
    internal sealed class SolverContext
    {
        #region Fields

        private Grid uniqueSolution = null;
        private List<Grid> solutions = new List<Grid>();
        private DateTime endTime;

        #endregion

        #region Public Properties

        public DateTime StartTime { get; set; }

        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                CalculationTime = endTime.Subtract(StartTime).TotalMilliseconds;
            }
        }

        public Grid UniqueSolution => uniqueSolution;
        public IReadOnlyList<Grid> Solutions => solutions.AsReadOnly();
        public double CalculationTime { get; set; }
        public int SearchCallCount { get; set; }
        public Grid Problem { get; set; }

        #endregion

        public void AddSolution(Grid grid)
        {
            if (solutions.Count == 0)
            {
                uniqueSolution = grid;
            }
            else
            {
                uniqueSolution = null;
            }

            solutions.Add(grid);
        }
    }
}
