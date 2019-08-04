using System;
using System.Collections.Generic;
using System.Linq;

namespace SudoEng
{
    /// <summary>
    /// Sudoku problem generator.
    /// </summary>
    /// <remarks>
    /// The problem generator performs a number of steps to generate a problem.
    /// The overall strategy is as follows
    /// 
    /// 1. Generate a terminal pattern that obeys the game restrictions using a Las Vegas
    /// approach to first randomly assign a number of given squares. Then the solver is
    /// invoked to generate a the final terminal pattern. If the solver cannot find a unique
    /// solution, a new randomized set of givens is set and this is repeated until a
    /// terminal pattern is found.
    /// 
    /// 2. To generate a problem from the terminal pattern, holes are dug using different
    /// strategys to walk along the grid. Depending on which difficulty level the problem
    /// shall get, there are restictions set on which holes can be dug. The solver is also
    /// invoked at each digging of a hole to ensure that the problem has a unique solution.
    /// The digging proceeds until no more holes can be dug.
    /// 
    /// 3. The problem and solution grids are 'propageted' be doing a permutations of columns
    /// and rows that obey the game rules. Also, a block/mini grid permutatin is performed.
    /// This is used to make sure that the final grid is more randomized since there is a
    /// small risk of the hole digger, tending to produce a somewhat uneven distribution of
    /// holes.
    /// 
    /// 4. The solver is once more run and on the final solution, the grader is run to
    /// estimate the difficulty level. This can be is used in output to compare the
    /// targeted difficulty with the generated difficulty.
    /// </remarks>
    internal sealed class Generator
    {
        #region Private Constants

        private const int InitialGivenCount = 11;
        private const int MinimumToDigFour = 61;
        private const int MinimumToDigTwo = 51;
        private const int MaxNumSquaresToDig = 4;

        #endregion

        #region Private Fields

        private Random random = new Random();
        private bool[] canBeDug;
        private int canBeDugCount;
        private int givenLowerBound;
        private int givenInRowAndColLowerBound;
        private int nextSquareInSequence;
        private Func<bool, int[]> nextSquaresFunction;

        #endregion

        #region Private Enums

        private enum DigSequence
        {
            LeftToRightTopToBottom,
            WanderingAlongS,
            JumpingOneCell,
            RandomizeGlobally,
        }

        #endregion

        #region Public Methods

        public Grid Generate(Difficulty difficulty, GeneratorContext generatorContext)
        {
            if (generatorContext == null) { throw new ArgumentNullException("generatorContext"); }

            DateTime startTime = DateTime.Now;
            Grid solutionGrid = GenerateTerminalPattern();
            Grid problemGrid = DigHoles(solutionGrid, difficulty);
            Propagate(problemGrid, solutionGrid);

            Solver solver = new Solver();
            SolverContext solverContext = new SolverContext();
            solver.Solve(problemGrid, solverContext);

            Grader grader = new Grader();
            Grading grading = grader.Grade(solverContext);

            generatorContext.Solution = solverContext.UniqueSolution;
            generatorContext.TargetedDifficulty = difficulty;
            generatorContext.GradedDifficulty = grading.GradedDifficulty;
            generatorContext.NumberOfGiven = problemGrid.GetNumberOfGiven();
            generatorContext.LowerBound = problemGrid.GetLowerBoundInRowAndCol();
            generatorContext.CalculationTime = DateTime.Now.Subtract(startTime).TotalMilliseconds;

            return problemGrid;
        }

        #endregion

        #region Private Static Methods

        private static SquareValue RandomSquareValue(Random random)
        {
            switch ((random.Next() % 9) + 1)
            {
                case 1: return SquareValue.One;
                case 2: return SquareValue.Two;
                case 3: return SquareValue.Three;
                case 4: return SquareValue.Four;
                case 5: return SquareValue.Five;
                case 6: return SquareValue.Six;
                case 7: return SquareValue.Seven;
                case 8: return SquareValue.Eight;
                case 9: return SquareValue.Nine;
            }

            return SquareValue.None;  // Does not happen.
        }

        private static int RandomSquare(Random random)
        {
            return random.Next() % Grid.SquareCount;
        }

        private static int RandomNumGivenLowerBound(Difficulty difficulty, Random random)
        {
            int rangeMin = 0;
            int rangeMax = 0;

            switch (difficulty)
            {
                case Difficulty.VeryEasy:
                    rangeMin = 50;
                    rangeMax = 60;
                    break;
                case Difficulty.Easy:
                    rangeMin = 36;
                    rangeMax = 49;
                    break;
                case Difficulty.Medium:
                    rangeMin = 32;
                    rangeMax = 35;
                    break;
                case Difficulty.Hard:
                    rangeMin = 28;
                    rangeMax = 31;
                    break;
                case Difficulty.Samurai:
                    rangeMin = 22;
                    rangeMax = 27;
                    break;
            }

            return random.Next() % (rangeMax - rangeMin + 1) + rangeMin;
        }

        private static int GetNumGivenInRowAndColLowerBound(Difficulty difficulty)
        {
            int ans = 5;

            switch (difficulty)
            {
                case Difficulty.VeryEasy:
                    ans = 5;
                    break;
                case Difficulty.Easy:
                    ans = 4;
                    break;
                case Difficulty.Medium:
                    ans = 3;
                    break;
                case Difficulty.Hard:
                    ans = 2;
                    break;
                case Difficulty.Samurai:
                    ans = 0;
                    break;
            }

            return ans;
        }

        private Func<bool, int[]> GetNextSquareFunction(Difficulty difficulty)
        {
            Func<bool, int[]> func = NextRandomizeGlobally;

            switch (difficulty)
            {
                case Difficulty.VeryEasy:
                case Difficulty.Easy:
                    func = NextRandomizeGlobally;
                    break;
                case Difficulty.Medium:
                    func = NextJumpingOneCell;
                    break;
                case Difficulty.Hard:
                case Difficulty.Samurai:
                    func = NextLeftToRightTopToBottom;
                    break;
            }

            return func;
        }

        private static int[] Generate0To2Permutation()
        {
            var random = new Random();
            var initial = new List<int>() { 0, 1, 2 };
            var permutation = new int[Grid.BlockSize];
            int k = 0;

            while (initial.Count != 0)
            {
                int r = random.Next() % initial.Count;
                permutation[k++] = initial.ElementAt(r);
                initial.RemoveAt(r);
            }

            return permutation;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Generate a valid terminal pattern using Las Vegas randomized approach, letting
        /// the solver produce the final pattern.
        /// </summary>
        /// <returns>The terminal pattern.</returns>
        private Grid GenerateTerminalPattern()
        {
            Grid grid = null;
            Reducer reducer = new Reducer();
            Solver solver = new Solver();
            bool solved = false;

            while (!solved)
            {
                int givenCount = 0;
                grid = new Grid();

                while (givenCount < InitialGivenCount)
                {
                    int square = RandomSquare(random);

                    if (!grid.IsSquareUnique(square))
                    {
                        SquareValue value = RandomSquareValue(random);
                        Grid tryGrid = (Grid)grid.Clone();
                        tryGrid.SetSquare(square, value);
                        tryGrid = tryGrid.Reduce();

                        if (tryGrid != null)
                        {
                            grid = tryGrid;
                            givenCount++;
                        }
                    }
                }

                SolverContext solverContext = new SolverContext();
                solver.Solve(grid, solverContext, true, TimeSpan.FromSeconds(5));

                if (solverContext.UniqueSolution != null)
                {
                    grid = solverContext.UniqueSolution;
                    solved = true;
                }
            }

            return grid;
        }

        /// <summary>
        /// Dig holes in the terminal pattern grid using a digging strategy based on the 
        /// difficulty level.
        /// </summary>
        /// <param name="grid">The terminal pattern to dig holes into.</param>
        /// <param name="difficulty">The difficuly determining how many wholes to dig and under what restrictions.</param>
        /// <returns>The grid with holes dug.</returns>
        private Grid DigHoles(Grid grid, Difficulty difficulty)
        {
            Grid problemGrid = new Grid(grid);

            // Here we set some restrictions on which holes that can be dug, based
            // on the targeted difficulty level.
            givenLowerBound = RandomNumGivenLowerBound(difficulty, random);
            givenInRowAndColLowerBound = GetNumGivenInRowAndColLowerBound(difficulty);

            canBeDugCount = Grid.SquareCount;
            canBeDug = Enumerable.Repeat(true, Grid.SquareCount).ToArray();

            // The 'next square' function determines in which pattern holes shall be dug.
            // The difficulty level determines the pattern. The patterns are also designed
            // so that the generated problems displays at least some level of symmetry.
            nextSquaresFunction = GetNextSquareFunction(difficulty);
            bool first = true;

            while (canBeDugCount > 0)
            {
                int numLeftToDig = 1;

                // We attempt to dig several squares at the same time with symmetry.
                // Symmetry is achieved by using opposite diagonals.
                int[] squaresToDig = nextSquaresFunction(first);

                first = false;

                if (squaresToDig[0] == -1)
                {
                    // This indicates that the used 'next square' function has exhausted all
                    // squares to dig so we now fall back to random squares for the final part.
                    nextSquaresFunction = NextRandomizeGlobally;
                    squaresToDig = nextSquaresFunction(first);
                }
                
                // Depending on how many holes there are left, we set how many holes to attempt
                // in order not to run into problems trying to dig too much when there are
                // more and more holes in the grid. We dig 1, 2, or 4 holes.
                if (canBeDugCount >= MinimumToDigFour)
                {
                    numLeftToDig = 4;
                }
                else if (canBeDugCount >= MinimumToDigTwo)
                {
                    numLeftToDig = 2;
                }

                while (numLeftToDig > 0)
                {
                    int square = squaresToDig[numLeftToDig - 1];

                    if (square >= 0)
                    {
                        if (canBeDug[square])
                        {
                            Grid tryGrid = (Grid)problemGrid.Clone();
                            tryGrid.SetSquare(square, SquareValue.All);

                            if (CheckRestrictions(tryGrid) && CheckUniqueSolution(tryGrid))
                            {
                                problemGrid = tryGrid;
                            }

                            canBeDug[square] = false;
                            canBeDugCount--;
                        }
                    }

                    numLeftToDig--;
                }
            }

            return problemGrid;
        }

        private void Propagate(Grid problemGrid, Grid solutionGrid)
        {
            PropagateColsAndRows(problemGrid, solutionGrid);
            PropagateBlocks(problemGrid, solutionGrid);
        }

        private void PropagateColsAndRows(Grid problemGrid, Grid solutionGrid)
        {
            var perm = Generate0To2Permutation();

            Grid colPropagatedProblemGrid = new Grid();
            Grid colPropagatedSolutionGrid = new Grid();
            Grid finalProblemGrid = new Grid();
            Grid finalSolutionGrid = new Grid();

            for (int i = 0; i < Grid.Size; i++)
            {
                for (int j = 0; j < Grid.Size; j++)
                {
                    int col = Grid.BlockSize * (j / Grid.BlockSize) + perm[j % Grid.BlockSize];
                    SquareValue problemValue = problemGrid.GetSquare(i, col);
                    SquareValue solutionValue = solutionGrid.GetSquare(i, col);
                    colPropagatedProblemGrid.SetSquare(i, j, problemValue);
                    colPropagatedSolutionGrid.SetSquare(i, j, solutionValue);
                }
            }

            for (int i = 0; i < Grid.Size; i++)
            {
                for (int j = 0; j < Grid.Size; j++)
                {
                    int row = Grid.BlockSize * (i / Grid.BlockSize) + perm[i % Grid.BlockSize];
                    SquareValue problemValue = colPropagatedProblemGrid.GetSquare(row, j);
                    SquareValue solutionValue = colPropagatedSolutionGrid.GetSquare(row, j);
                    finalProblemGrid.SetSquare(i, j, problemValue);
                    finalSolutionGrid.SetSquare(i, j, solutionValue);
                }
            }

            problemGrid.Set(finalProblemGrid);
            solutionGrid.Set(finalSolutionGrid);
        }

        private void PropagateBlocks(Grid problemGrid, Grid solutionGrid)
        {
            var perm = Generate0To2Permutation();

            Grid colBlockPropagatedProblemGrid = new Grid();
            Grid colBlockPropagatedSolutionGrid = new Grid();
            Grid finalProblemGrid = new Grid();
            Grid finalSolutionGrid = new Grid();

            for (int i = 0; i < Grid.Size; i++)
            {
                for (int b = 0; b < (Grid.Size / Grid.BlockSize); b++)
                {
                    for (int j = 0; j < Grid.BlockSize; j++)
                    {
                        int permBlockCol = perm[b];
                        SquareValue problemValue = problemGrid.GetSquare(i, b * Grid.BlockSize + j);
                        SquareValue solutionValue = solutionGrid.GetSquare(i, b * Grid.BlockSize + j);
                        colBlockPropagatedProblemGrid.SetSquare(i, permBlockCol * Grid.BlockSize + j, problemValue);
                        colBlockPropagatedSolutionGrid.SetSquare(i, permBlockCol * Grid.BlockSize + j, solutionValue);
                    }
                }
            }

            for (int b = 0; b < (Grid.Size / Grid.BlockSize); b++)
            {
                for (int i = 0; i < Grid.BlockSize; i++)
                {
                    for (int j = 0; j < Grid.Size; j++)
                    {
                        int permBlockRow = perm[b];
                        SquareValue problemValue = colBlockPropagatedProblemGrid.GetSquare(b * Grid.BlockSize + i, j);
                        SquareValue solutionValue = colBlockPropagatedSolutionGrid.GetSquare(b * Grid.BlockSize + i, j);
                        finalProblemGrid.SetSquare(permBlockRow * Grid.BlockSize + i, j, problemValue);
                        finalSolutionGrid.SetSquare(permBlockRow * Grid.BlockSize + i, j, solutionValue);
                    }
                }
            }

            problemGrid.Set(finalProblemGrid);
            solutionGrid.Set(solutionGrid);
        }

        private int[] NextJumpingOneCell(bool reset)
        {
            int[] ans = new int[MaxNumSquaresToDig];

            if (reset)
            {
                nextSquareInSequence = 0;
            }
            else
            {
                int row = nextSquareInSequence / Grid.Size;
                int col = nextSquareInSequence % Grid.Size;

                if (row % 2 == 0)
                {
                    if (col < Grid.Size - 1)
                    {
                        nextSquareInSequence += 2;
                    }
                    else
                    {
                        nextSquareInSequence += (Grid.Size - 1);
                    }
                }
                else
                {
                    if (col > 2)
                    {
                        nextSquareInSequence -= 2;
                    }
                    else
                    {
                        nextSquareInSequence += (Grid.Size - 1);
                    }
                }

                if (nextSquareInSequence >= Grid.SquareCount)
                {
                    nextSquareInSequence = -1;
                }
            }

            ans[0] = nextSquareInSequence;
            AddDiagnalOpposites(ans);
            return ans;
        }

        private int[] NextLeftToRightTopToBottom(bool reset)
        {
            int[] ans = new int[MaxNumSquaresToDig];

            if (reset)
            {
                nextSquareInSequence = 0;
            }
            else
            {
                nextSquareInSequence++;

                if (nextSquareInSequence >= Grid.SquareCount)
                {
                    nextSquareInSequence = -1;
                }
            }

            ans[0] = nextSquareInSequence;
            AddDiagnalOpposites(ans);
            return ans;
        }

        private int[] NextRandomizeGlobally(bool reset)
        {
            int[] ans = new int[MaxNumSquaresToDig];
            int steps = random.Next() % (Grid.SquareCount + 1);
            int k = 0;

            while (steps > 0)
            {
                if (k == Grid.SquareCount)
                {
                    k = 0;
                }

                if (canBeDug[k])
                {
                    steps--;

                    if (steps == 0)
                    {
                        nextSquareInSequence = k;
                    }
                }

                k++;
            }

            ans[0] = nextSquareInSequence;
            ans[1] = -1;
            ans[2] = -1;
            ans[3] = -1;

            return ans;
        }

        private void AddDiagnalOpposites(int[] squares)
        {
            if (squares[0] != -1)
            {
                int row = squares[0] / Grid.Size;
                int col = squares[0] % Grid.Size;
                int newRow = (Grid.Size - 1) - col;
                int newCol = (Grid.Size - 1) - row;
                squares[1] = newRow * Grid.Size + newCol;

                row = newRow;
                col = newCol;
                newRow = col;
                newCol = row;
                squares[2] = newRow * Grid.Size + newCol;

                row = newRow;
                col = newCol;
                newRow = (Grid.Size - 1) - col;
                newCol = (Grid.Size - 1) - row;
                squares[3] = newRow * Grid.Size + newCol;
            }
        }
        
        private bool CheckRestrictions(Grid grid)
        {
            return (grid.GetNumberOfGiven() >= givenLowerBound) &&
                   (grid.GetLowerBoundInRowAndCol() >= givenInRowAndColLowerBound);
        }

        public bool CheckUniqueSolution(Grid grid)
        {
            Solver solver = new Solver();
            SolverContext context = new SolverContext();

            solver.Solve(grid, context);
            return (context.UniqueSolution != null);
        }

        #endregion
    }
}
