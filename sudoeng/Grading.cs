using System;

namespace SudoEng
{
    /// <summary>
    /// Represents a grading of a Sudoku problem.
    /// </summary>
    internal sealed class Grading
    {
        private double score;
        private int numGivenScore;
        private int lowerBoundScore;
        private int numSearchCallsScore;
        private Difficulty gradedDifficulty;

        public double Score => score;
        public int NumGivenScore => numGivenScore;
        public int LowerBoundScore => lowerBoundScore;
        public int NumSearchCallsScore => numSearchCallsScore;
        public Difficulty GradedDifficulty => gradedDifficulty;

        public Grading(double score, int numGivenScore, int lowerBoundScore, int numSearchCallsScore)
        {
            this.score = score;
            this.numGivenScore = numGivenScore;
            this.lowerBoundScore = lowerBoundScore;
            this.numSearchCallsScore = numSearchCallsScore;
            this.gradedDifficulty = (Difficulty)Math.Round(score);
        }
    }
}
