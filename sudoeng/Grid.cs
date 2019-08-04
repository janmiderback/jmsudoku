using System;
using System.Collections.Generic;
using System.Text;

namespace SudoEng
{
    /// <summary>
    /// Represents a Sudoku grid with given and underermined numbers.
    /// </summary>
    /// <remarks>
    /// Each square value is represented by the <see cref="SquareValue"/> enumeration providing
    /// a bit field that can represent a square with both a final given value or which set of
    /// possible values the square can contain.
    /// </remarks>
    internal sealed class Grid : ICloneable
    {
        #region Fields

        public const int Size = 9;
        public const int BlockSize = 3;
        public const int SquareCount = Size * Size;
        public const int NumSquaresInBlock = BlockSize * BlockSize;

        private SquareValue[,] values = new SquareValue[Size, Size];

        #endregion
                
        #region Private Static Methods

        private static bool IsSquareUnique(SquareValue value)
        {
            return value == SquareValue.One ||
                   value == SquareValue.Two ||
                   value == SquareValue.Three ||
                   value == SquareValue.Four ||
                   value == SquareValue.Five ||
                   value == SquareValue.Six ||
                   value == SquareValue.Seven ||
                   value == SquareValue.Eight ||
                   value == SquareValue.Nine;
        }

        private static List<SquareValue> SplitValues(SquareValue value)
        {
            var values = new List<SquareValue>();

            if ((value & SquareValue.One) == SquareValue.One) values.Add(SquareValue.One);
            if ((value & SquareValue.Two) == SquareValue.Two) values.Add(SquareValue.Two);
            if ((value & SquareValue.Three) == SquareValue.Three) values.Add(SquareValue.Three);
            if ((value & SquareValue.Four) == SquareValue.Four) values.Add(SquareValue.Four);
            if ((value & SquareValue.Five) == SquareValue.Five) values.Add(SquareValue.Five);
            if ((value & SquareValue.Six) == SquareValue.Six) values.Add(SquareValue.Six);
            if ((value & SquareValue.Seven) == SquareValue.Seven) values.Add(SquareValue.Seven);
            if ((value & SquareValue.Eight) == SquareValue.Eight) values.Add(SquareValue.Eight);
            if ((value & SquareValue.Nine) == SquareValue.Nine) values.Add(SquareValue.Nine);

            return values;
        }

        private static string ConvertToString(SquareValue value)
        {
            string ans = ".";

            switch (value)
            {
                case SquareValue.One:
                    ans = "1";
                    break;
                case SquareValue.Two:
                    ans = "2";
                    break;
                case SquareValue.Three:
                    ans = "3";
                    break;
                case SquareValue.Four:
                    ans = "4";
                    break;
                case SquareValue.Five:
                    ans = "5";
                    break;
                case SquareValue.Six:
                    ans = "6";
                    break;
                case SquareValue.Seven:
                    ans = "7";
                    break;
                case SquareValue.Eight:
                    ans = "8";
                    break;
                case SquareValue.Nine:
                    ans = "9";
                    break;
            }

            return ans;
        }

        #endregion

        #region Public Methods

        public Grid()
        {
            for (int s = 0; s < SquareCount; s++)
            { 
                SetSquare(s, SquareValue.All);
            }
        }

        public Grid(Grid that) : base()
        {
            this.values = (SquareValue[,])that.values.Clone();
        }

        #region ICloneable

        public object Clone()
        {
            return new Grid(this);
        }

        #endregion

        public void Set(Grid grid)
        {
            for (int s = 0; s < SquareCount; s++)
            {
                SetSquare(s, grid.GetSquare(s));
            }
        }

        public void SetSquare(int row, int col, SquareValue value)
        {
            this.values[row, col] = value;
        }

        public void SetSquare(int square, SquareValue value)
        {
            SetSquare(square / Size, square % Size, value);
        }

        public SquareValue GetSquare(int row, int col)
        {
            return this.values[row, col];
        }

        public SquareValue GetSquare(int square)
        {
            return values[square / Size, square % Size];
        }

        public int GetNumberOfGiven()
        {
            int ans = 0;

            for (int s = 0; s < SquareCount; s++)
            {
                if (IsSquareUnique(s)) { ans++; }
            }

            return ans;
        }

        public int GetLowerBoundInRowAndCol()
        {
            int ans = int.MaxValue;

            for (int i = 0; i < Grid.Size; i++)
            {
                int numOfGivenInRow = 0;

                for (int j = 0; j < Grid.Size; j++)
                {
                    if (IsSquareUnique(i, j))
                    {
                        numOfGivenInRow++;
                    }
                }

                if (numOfGivenInRow < ans)
                {
                    ans = numOfGivenInRow;
                }
            }

            for (int j = 0; j < Grid.Size; j++)
            {
                int numOfGivenInCol = 0;

                for (int i = 0; i < Grid.Size; i++)
                {
                    if (IsSquareUnique(i, j))
                    {
                        numOfGivenInCol++;
                    }
                }

                if (numOfGivenInCol < ans)
                {
                    ans = numOfGivenInCol;
                }
            }

            return ans;
        }

        public bool IsSquareUnique(int row, int col)
        {
            return IsSquareUnique(values[row, col]);
        }

        public bool IsSquareUnique(int square)
        {
            return IsSquareUnique(values[square / Size, square % Size]);
        }

        public bool AreAllUnique()
        {
            bool allUnique = true;

            for (int s = 0; allUnique && s < SquareCount; s++)
            {
                allUnique = allUnique && IsSquareUnique(s);
            }

            return allUnique;
        }

        public void ResetAllNonUniqe()
        {
            for (int s = 0; s < SquareCount; s++)
            {
                if (!IsSquareUnique(s))
                {
                    SetSquare(s, SquareValue.All);
                }
            }
        }

        public bool IsSolution()
        {
            bool isSolution = false;

            if (AreAllUnique())
            {
                Reducer reducer = new Reducer();
                isSolution = (reducer.Reduce(this) != null);
            }

            return isSolution;
        }

        public Grid Reduce()
        {
            Reducer reducer = new Reducer();
            return reducer.Reduce(this);
        }

        public IEnumerable<Grid> GenerateMoves()
        {
            List<Grid> moves = null;
            List<SquareValue> minValues = null;
            int square = 0;
            
            for (int s = 0; s < SquareCount; s++)
            {
                List<SquareValue> values = SplitValues(this.GetSquare(s));

                if (values.Count >= 2)
                {
                    if ((minValues == null) || (values.Count < minValues.Count))
                    {
                        square = s;
                        minValues = values;
                    }
                }
            }

            if (minValues != null)
            {
                Reducer reducer = new Reducer();
                moves = new List<Grid>();

                foreach (var value in minValues)
                {
                    var move = new Grid(this);
                    move.SetSquare(square, value);

                    if (reducer.Reduce(square, move))
                    {
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            for (int row = 0; row < Grid.Size; row++)
            {
                for (int col = 0; col < Grid.Size; col++)
                {
                    builder.Append(Grid.ConvertToString(this.values[row, col]));
                }

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        public string ToDebugString()
        {
            StringBuilder builder = new StringBuilder();

            for (int row = 0; row < Grid.Size; row++)
            {
                for (int col = 0; col < Grid.Size; col++)
                {
                    SquareValue value = GetSquare(row, col);
                    var values = SplitValues(value);
                    
                    foreach (var v in values)
                    {
                        builder.Append(Grid.ConvertToString(v));
                    }

                    builder.Append(' ', Size - values.Count);
                    builder.Append(',');
                }

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

#endregion
    }
}
