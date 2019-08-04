namespace SudoEng
{
    /// <summary>
    /// Grader of a Sudoku problem.
    /// </summary>
    /// <remarks>
    /// The grader uses three measures to determine the difficulty level
    /// 1. The number of given squares in the problem.
    /// 2. The lower bound of given in rows and columns.
    /// 3. The number of search calls the solver did to completely evaluate the problem.
    /// </remarks>
    internal sealed class Grader
    {
        public Grading Grade(SolverContext solverContext)
        {
            int numGiven = solverContext.Problem.GetNumberOfGiven();
            int lowerBound = solverContext.Problem.GetLowerBoundInRowAndCol();
            int numSearchCalls = solverContext.SearchCallCount;

            int numGivenScore;
            int lowerBoundScore;
            int numSearchCallsScore;

            if (numGiven >= 50) { numGivenScore = 1; }
            else if (numGiven >= 36) { numGivenScore = 2; }
            else if (numGiven >= 32) { numGivenScore = 3; }
            else if (numGiven >= 28) { numGivenScore = 4; }
            else { numGivenScore = 5; }

            if (lowerBound >= 5) { lowerBoundScore = 1; }
            else if (lowerBound == 4) { lowerBoundScore = 2; }
            else if (lowerBound == 3) { lowerBoundScore = 3; }
            else if (lowerBound >= 1) { lowerBoundScore = 4; }
            else { lowerBoundScore = 5; }

            if (numSearchCalls < 100) { numSearchCallsScore = 1; }
            else if (numSearchCalls < 1000) { numSearchCallsScore = 2; }
            else if (numSearchCalls < 10000) { numSearchCallsScore = 3; }
            else if (numSearchCalls < 100000) { numSearchCallsScore = 4; }
            else { numSearchCallsScore = 5; }

            double score = 0.5 * numGivenScore + 0.3 * lowerBoundScore + 0.2 * numSearchCallsScore;

            return new Grading(score, numGivenScore, lowerBoundScore, numSearchCallsScore);
        }
    }
}
