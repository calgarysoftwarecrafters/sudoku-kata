using System;
using System.Linq;

namespace SudokuKata
{
    public class SudokuBoard
    {
        public SudokuBoard()
        {
            Board = GetEmptyBoard();
        }

        private char[][] Board { get; }

        private char[][] GetEmptyBoard()
        {
            var line = "+---+---+---+";
            var middle = "|...|...|...|";
            return new[]
            {
                line.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                line.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                line.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                line.ToCharArray()
            };
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Board.Select(s => new string(s)).ToArray());
        }

        public void SetElementAt(int row, int col, int digitValue)
        {
            char boardValue;
            if (digitValue == Unknown)
            {
                boardValue = '.';
            }
            else
            {
                boardValue = digitValue.ToString().Single();
            }

            Board[row][col] = boardValue;
        }

        public const int Unknown = -1;

        public string DisplayBoardWithEmptyChar()
        {
            return string.Join(string.Empty, Board.Select(s => new string(s)).ToArray())
                .Replace("-", string.Empty)
                .Replace("+", string.Empty)
                .Replace("|", string.Empty)
                .Replace(".", "0");
        }
    }
}