namespace SudoEng
{
    /// <summary>
    /// Generator context providing information on the generator run.
    /// </summary>
    internal sealed class GeneratorContext
    {
        public Grid Solution { get; set; }
        public double CalculationTime { get; set; }
        public double NumberOfGiven { get; set; }
        public double LowerBound { get; set; }
        public Difficulty TargetedDifficulty { get; set; }
        public Difficulty GradedDifficulty { get; set; }
    }
}
