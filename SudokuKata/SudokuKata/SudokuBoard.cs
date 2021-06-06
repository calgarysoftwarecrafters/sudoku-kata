using System;
using System.Linq;

namespace SudokuKata
{
    static class _
    {
        public static int[][] SetAll(this int[][] that, int value)
        {
            for (int row = 0; row < SudokuBoard.NumRows; row++)
            {
                for (int col = 0; col < SudokuBoard.NumCols; col++)
                {
                    that[row][col] = value;
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
        private int[][] Board2 { get; } = new int[NumRows][].SetAll(Unknown);

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

        public void SetElementAtWithRowColCalc(int row, int col, int digitValue)
        {
            var rowToWrite = row + row / 3 + 1;
            var colToWrite = col + col / 3 + 1;
            char boardValue;
            if (digitValue == Unknown)
            {
                boardValue = '.';
            }
            else
            {
                boardValue = digitValue.ToString().Single();
            }

            Board[rowToWrite][colToWrite] = boardValue;
            Board2[row][col] = digitValue;
        }

        
        public const int Unknown = -1;
        public const int NumRows = 9;
        public const int NumCols = 9;

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