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
        private int[][] Board { get; } = new int[NumRows][].SetAll(Unknown);

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
                result += ToRowString(Board[row]) + Environment.NewLine;
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

        public void SetValueAt(int row, int col, int digitValue)
        {
            Board[row][col] = digitValue;
        }

        
        public const int Unknown = -1;
        public const int NumRows = 9;
        public const int NumCols = 9;

        public string DisplayBoard()
        {
            return string.Join(string.Empty, Board.Select(rowDigits =>
            {
                var joinedRow = string.Join(string.Empty, rowDigits.Select(digit => digit == Unknown ? 0 : digit));
                return joinedRow;
            }));
        }
    }
}