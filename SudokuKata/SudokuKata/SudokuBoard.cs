using System;
using System.Linq;

namespace SudokuKata
{
    static class _
    {
        public static int[,] SetAll(this int[,] that, int value)
        {
            var numRows = that.GetLength(0);
            var numCols = that.GetLength(1);
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    that[row, col] = value;
                }
            }
            return that;
        }
    }

    public class SudokuBoard
    {
        public SudokuBoard()
        {
            Board = GetEmptyBoard();
        }

        private char[][] Board { get; }
        private int[,] Board2 { get; } = new int[9,9].SetAll(Unknown);

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