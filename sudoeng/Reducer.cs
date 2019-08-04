namespace SudoEng
{
    /// <summary>
    /// Reduces a Sudoku grid by doing elemination of possible values in rows, columns and
    /// minigrids.
    /// </summary>
    internal sealed class Reducer
    {
        #region Fields

        private Grid reducedState = null;

        private static readonly int[][,] blocks = new int[Grid.SquareCount][,];

        private static readonly int[,] block11 =
            { { 0, 0 }, { 0, 1 }, { 0, 2 },
              { 1, 0 }, { 1, 1 }, { 1, 2 },
              { 2, 0 }, { 2, 1 }, { 2, 2 } };

        private static readonly int[,] block12 =
            { { 0, 3 }, { 0, 4 }, { 0, 5 },
              { 1, 3 }, { 1, 4 }, { 1, 5 },
              { 2, 3 }, { 2, 4 }, { 2, 5 } };

        private static readonly int[,] block13 =
            { { 0, 6 }, { 0, 7 }, { 0, 8 },
              { 1, 6 }, { 1, 7 }, { 1, 8 },
              { 2, 6 }, { 2, 7 }, { 2, 8 } };

        private static readonly int[,] block21 =
            { { 3, 0 }, { 3, 1 }, { 3, 2 },
              { 4, 0 }, { 4, 1 }, { 4, 2 },
              { 5, 0 }, { 5, 1 }, { 5, 2 } };

        private static readonly int[,] block22 =
            { { 3, 3 }, { 3, 4 }, { 3, 5 },
              { 4, 3 }, { 4, 4 }, { 4, 5 },
              { 5, 3 }, { 5, 4 }, { 5, 5 } };

        private static readonly int[,] block23 =
            { { 3, 6 }, { 3, 7 }, { 3, 8 },
              { 4, 6 }, { 4, 7 }, { 4, 8 },
              { 5, 6 }, { 5, 7 }, { 5, 8 } };

        private static readonly int[,] block31 =
            { { 6, 0 }, { 6, 1 }, { 6, 2 },
              { 7, 0 }, { 7, 1 }, { 7, 2 },
              { 8, 0 }, { 8, 1 }, { 8, 2 } };

        private static readonly int[,] block32 =
            { { 6, 3 }, { 6, 4 }, { 6, 5 },
              { 7, 3 }, { 7, 4 }, { 7, 5 },
              { 8, 3 }, { 8, 4 }, { 8, 5 } };

        private static readonly int[,] block33 =
            { { 6, 6 }, { 6, 7 }, { 6, 8 },
              { 7, 6 }, { 7, 7 }, { 7, 8 },
              { 8, 6 }, { 8, 7 }, { 8, 8 } };

        #endregion

        static Reducer()
        {
            for (int s = 0; s < Grid.SquareCount; s++)
            {
                int row = s / Grid.Size;
                int col = s % Grid.Size;
                int[,] block = null;

                if (0 <= row && row <= 2)
                {
                    if (0 <= col && col <= 2) { block = block11; }
                    else if (3 <= col && col <= 5) { block = block12; }
                    else if (6 <= col && col <= 8) { block = block13; }
                }
                else if (3 <= row && row <= 5)
                {
                    if (0 <= col && col <= 2) { block = block21; }
                    else if (3 <= col && col <= 5) { block = block22; }
                    else if (6 <= col && col <= 8) { block = block23; }
                }
                else if (6 <= row && row <= 8)
                {
                    if (0 <= col && col <= 2) { block = block31; }
                    else if (3 <= col && col <= 5) { block = block32; }
                    else if (6 <= col && col <= 8) { block = block33; }
                }

                blocks[s] = block;
            }
        }

        #region Public Methods

        public Grid Reduce(Grid originalState)
        {
            bool valid = true;
            reducedState = new Grid(originalState);

            for (int s = 0; valid && s < Grid.SquareCount; s++)
            {
                if (originalState.IsSquareUnique(s))
                {
                    SquareValue value = originalState.GetSquare(s);
                    valid = ReducePeers(s, value);
                }
            }

            return valid ? reducedState : null;
        }

        public bool Reduce(int square, Grid grid)
        {
            SquareValue reduceValue = grid.GetSquare(square);
            this.reducedState = grid;
            bool valid = this.ReduceRow(square, reduceValue);
            valid = valid && this.ReduceCol(square, reduceValue);
            return valid && this.ReduceBlock(square, reduceValue);
        }

        #endregion

        #region Private Methods

        private bool ReducePeers(int square, SquareValue reduceValue)
        {
            bool valid = this.ReduceRow(square, reduceValue);
            valid = valid && this.ReduceCol(square, reduceValue);
            return valid && this.ReduceBlock(square, reduceValue);
        }

        private bool ReduceRow(int square, SquareValue reduceValue)
        {
            int row = square / Grid.Size;
            int col = square % Grid.Size;
            bool valid = true;

            for (int j = 0; valid && j < Grid.Size; j++)
            {
                if (j != col)
                {
                    SquareValue newValue = reducedState.GetSquare(row, j) & ~reduceValue;
                    valid = (newValue != SquareValue.None);

                    if (valid)
                    {
                        reducedState.SetSquare(row, j, newValue);
                    }                    
                }
            }

            return valid;
        }

        private bool ReduceCol(int square, SquareValue reduceValue)
        {
            int row = square / Grid.Size;
            int col = square % Grid.Size;
            bool valid = true;

            for (int i = 0; valid && i < Grid.Size; i++)
            {
                if (i != row)
                {
                    SquareValue newValue = reducedState.GetSquare(i, col) & ~reduceValue;
                    valid = (newValue != SquareValue.None);

                    if (valid)
                    {
                        this.reducedState.SetSquare(i, col, newValue);
                    }
                }
            }

            return valid;
        }

        private bool ReduceBlock(int square, SquareValue reduceValue)
        {
            int row = square / Grid.Size;
            int col = square % Grid.Size;
            bool valid = true;
            int[,] block = this.GetBlock(square);

            for (int k = 0; valid && k < Grid.NumSquaresInBlock; k++)
            {
                int i = block[k, 0];
                int j = block[k, 1];

                if (i != row || j != col)
                {
                    SquareValue newValue = reducedState.GetSquare(i, j) & ~reduceValue;
                    valid = (newValue != SquareValue.None);

                    if (valid)
                    {
                        reducedState.SetSquare(i, j, newValue);
                    }
                }
            }

            return valid;
        }

        private int[,] GetBlock(int square)
        {
            return blocks[square];
        }

        #endregion
    }
}
