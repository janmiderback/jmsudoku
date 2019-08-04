using System;
using System.IO;

namespace SudoEng
{
    /// <summary>
    /// Parser of a Soduko problem.
    /// </summary>
    /// <remarks>
    /// Parses the problem as input text given in nine rows with nine digits in each row.
    /// Each row shall be terminated with newline characters.
    /// </remarks>
    internal sealed class Parser
    {
        public Grid Parse(TextReader reader)
        {
            if (reader == null) { throw new ArgumentNullException(); }

            Grid state = new Grid();

            for (int row = 0; row < Grid.Size; row++)
            {
                string line = reader.ReadLine();

                if (line.Length != Grid.Size) { throw new InvalidDataException("Invalid input line: " + line); }

                for (int col = 0; col < Grid.Size; col++)
                {
                    state.SetSquare(row, col, Parser.ConvertToSquareValue(line[col]));
                }
            }

            reader.Close();
            return state;
        }

        private static SquareValue ConvertToSquareValue(char val)
        {
            SquareValue convertedValue = SquareValue.All;

            switch (val)
            {
                case '1':
                    convertedValue = SquareValue.One;
                    break;
                case '2':
                    convertedValue = SquareValue.Two;
                    break;
                case '3':
                    convertedValue = SquareValue.Three;
                    break;
                case '4':
                    convertedValue = SquareValue.Four;
                    break;
                case '5':
                    convertedValue = SquareValue.Five;
                    break;
                case '6':
                    convertedValue = SquareValue.Six;
                    break;
                case '7':
                    convertedValue = SquareValue.Seven;
                    break;
                case '8':
                    convertedValue = SquareValue.Eight;
                    break;
                case '9':
                    convertedValue = SquareValue.Nine;
                    break;
                default:
                    break;
            }

            return convertedValue;
        }
    }
}
