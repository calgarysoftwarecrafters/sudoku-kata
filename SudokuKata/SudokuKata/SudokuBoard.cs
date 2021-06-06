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
                that[row] = new int [SudokuBoard.NumCols];
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
            var line = "+---+---+---+";
            var result = "";
            for (int row = 0; row < NumRows; row++)
            {
                if (row % 3 == 0)
                {
                    result += line + Environment.NewLine;
                }
                result += ToRowString(Board2[row]) + Environment.NewLine;
            }

            result += line;            
            return result;
        }

        private string ToRowString(int[] rowDigits)
        {
            var result = "";
            for (int index = 0; index < rowDigits.Length; index++)
            {
                if (index % 3 == 0)
                {
                    result += "|";
                }
                result += rowDigits[index] == Unknown ? "." : rowDigits[index].ToString();
            }

            result += "|";
            return result;
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
            return string.Join(string.Empty, Board2.Select(rowDigits =>
            {
                var joinedRow = string.Join(string.Empty, rowDigits.Select(digit => digit == Unknown ? 0 : digit));
                return joinedRow;
            }));
        }
    }
}