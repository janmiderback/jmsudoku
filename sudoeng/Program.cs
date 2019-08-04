using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SudoEng
{
    /// <summary>
    /// Program class containing the program entry point.
    /// </summary>
    sealed class Program
    {
        #region Constants

        private const string UsageStr = "Error. Usage:\r\n" +
                                        "To solve a Sudoku problem:    sodueng -s <filepath>\r\n" +
                                        "To generate a Sudoku problem: sodueng -g [veryeasy|easy|medium|hard|samurai]\r\n" +
                                        "All results are written to standard output.";
        private const string UniqueStr = "UNIQUE";
        private const string NotUniqueStr = "NOT UNIQUE";
        private const string NoSolutionStr = "NO SOLUTION";
        private const string AllSolutionsStr = "ALL SOLUTIONS";
        private const string ThinDividerStr = "----------------------------------------";
        private const string ThickDividerStr = "========================================";
        private const string CalculationTimeStr0 = "Calculation time: {0} ms";
        private const string SearchCallsStr0 = "Search calls: {0}";
        private const string NumberOfGivenStr0 = "Number of given: {0}";
        private const string LowerBoundStr0 = "Lower bound: {0}";
        private const string InputProblemStr0 = "Input problem: {0}";
        private const string GradedDifficultyStr0 = "Graded difficulty: {0}";
        private const string TargetDifficultyStr0 = "Targeted difficulty: {0}";
        private const string GradingNumGivenScoreStr0 = "Score - Number of given: {0}";
        private const string GradingLowerBoundScoreStr0 = "Score - Lower bound: {0}";
        private const string GradingNumSearchCallsScoreStr0 = "Score - Number of search calls: {0}";
        private const string GradingWeightedScoreValueStr0 = "Weighted Score value: {0}";
        private const string SolutionStr = "Solution:";
        private const string SolveOptionStr = "-s";
        private const string GenerateOptionStr = "-g";
        private const string VeryEasyStr = "veryeasy";
        private const string EasyStr = "easy";
        private const string MediumStr = "medium";
        private const string HardStr = "hard";
        private const string SamuraiStr = "samurai";
        private const string UnexpectedErrorStr = "Unexpected error. Check the input.";

        #endregion

        public static void Main(string[] args)
        {
            if ((args.Length == 0) ||
                (args[0].ToLower() == GenerateOptionStr && args.Length == 1) ||
                (args[0].ToLower() == SolveOptionStr && args.Length == 1))
            {
                WriteUsage();
                return;
            }

            if (args[0].ToLower() == SolveOptionStr)
            {
                Solve(args[1]);
            }
            else if (args[0].ToLower() == GenerateOptionStr)
            {
                Difficulty difficulty = ConvertToDifficulty(args[1]);

                if (difficulty == Difficulty.Unknown)
                {
                    WriteUsage();
                }
                else
                {
                    Generate(difficulty);
                }

            }
            else
            {
                WriteUsage();
            }
        }

        #region Private Static Methods

        private static Difficulty ConvertToDifficulty(string difficulty)
        {
            switch (difficulty.ToLower())
            {
                case VeryEasyStr: return Difficulty.VeryEasy;
                case EasyStr:     return Difficulty.Easy;
                case MediumStr:   return Difficulty.Medium;
                case HardStr:     return Difficulty.Hard;
                case SamuraiStr:  return Difficulty.Samurai;
            }

            return Difficulty.Unknown;
        }

        private static void Solve(string filepath)
        {
            try
            {
                Parser parser = new Parser();
                Grid grid = parser.Parse(File.OpenText(filepath));
                Solver solver = new Solver();
                SolverContext context = new SolverContext();
                Grading grading = null;

                solver.Solve(grid, context);

                if (context.UniqueSolution != null)
                {
                    Grader grader = new Grader();
                    grading = grader.Grade(context);
                }

                WriteSolution(filepath, context, grading);
            }
            catch
            {
                Console.WriteLine(UnexpectedErrorStr);
            }
        }

        private static void Generate(Difficulty difficulty)
        {
            Generator generator = new Generator();
            GeneratorContext generatorContext = new GeneratorContext();
            Grid grid = generator.Generate(difficulty, generatorContext);
            WriteGeneration(grid, generatorContext);
        }

        private static void WriteUsage()
        {
            Console.WriteLine(UsageStr);
        }

        private static void WriteGeneration(Grid grid, GeneratorContext generatorContext)
        {
            Console.Write(grid);
            WriteGenerationInfo(generatorContext);
        }

        private static void WriteGenerationInfo(GeneratorContext generatorContext)
        {
            Console.WriteLine(ThickDividerStr);
            Console.WriteLine(SolutionStr);
            Console.Write(generatorContext.Solution);
            Console.WriteLine(string.Format(CalculationTimeStr0, Math.Round(generatorContext.CalculationTime)));
            Console.WriteLine(string.Format(TargetDifficultyStr0, generatorContext.TargetedDifficulty));
            Console.WriteLine(string.Format(GradedDifficultyStr0, generatorContext.GradedDifficulty));
            Console.WriteLine(string.Format(NumberOfGivenStr0, generatorContext.NumberOfGiven));
            Console.WriteLine(string.Format(LowerBoundStr0, generatorContext.LowerBound));
            Console.WriteLine(ThickDividerStr);
        }

        private static void WriteSolution(string filepath, SolverContext solverContext, Grading grading)
        {
            int solutionCount = solverContext.Solutions.Count;

            if (solverContext.UniqueSolution != null)
            {
                WriteUniqueSolution(solverContext.UniqueSolution);
            }
            else if (solutionCount == 0)
            {
                WriteNoSolution();
            }
            else
            {
                WriteMultipleSolutions(solverContext.Solutions);
            }

            WriteSolutionInfo(filepath, solverContext, grading);
        }

        private static void WriteUniqueSolution(Grid solution)
        {
            Console.WriteLine(UniqueStr);
            Console.Write(solution);
        }

        private static void WriteMultipleSolutions(IReadOnlyList<Grid> solutions)
        {
            Console.WriteLine(NotUniqueStr);
            Console.Write(solutions.ElementAt(0));
            Console.WriteLine(AllSolutionsStr);

            foreach (var solution in solutions)
            {
                Console.Write(solution);
                Console.WriteLine(ThinDividerStr);
            }            
        }

        private static void WriteNoSolution()
        {
            Console.WriteLine(NoSolutionStr);
        }

        private static void WriteSolutionInfo(string filepath, SolverContext context, Grading grading)
        {
            Console.WriteLine(ThickDividerStr);
            Console.WriteLine(string.Format(CalculationTimeStr0, Math.Round(context.CalculationTime)));
            Console.WriteLine(string.Format(SearchCallsStr0, context.SearchCallCount));
            Console.WriteLine(string.Format(NumberOfGivenStr0, context.Problem.GetNumberOfGiven()));
            Console.WriteLine(string.Format(InputProblemStr0, filepath));
            Console.Write(context.Problem);

            if (grading != null)
            {
                Console.WriteLine(string.Format(GradedDifficultyStr0, grading.GradedDifficulty));
                Console.WriteLine(string.Format(GradingNumGivenScoreStr0, grading.NumGivenScore));
                Console.WriteLine(string.Format(GradingLowerBoundScoreStr0, grading.LowerBoundScore));
                Console.WriteLine(string.Format(GradingNumSearchCallsScoreStr0, grading.NumSearchCallsScore));
                Console.WriteLine(string.Format(GradingWeightedScoreValueStr0, grading.Score));
            }

            Console.WriteLine(ThickDividerStr);
        }

        #endregion
    }
}
