using System;
using System.Linq;

namespace SudokuKata
{
    internal static class _
    {
        public static int[][] SetAll(this int[][] that, int value)
        {
            for (var row = 0; row < SudokuBoard.NumRows; row++)
            {
                that[row] = new int [SudokuBoard.NumCols];
                for (var col = 0; col < SudokuBoard.NumCols; col++) that[row][col] = value;
            }

            return that;
        }
    }

    public class SudokuBoard
    {
        public const int Unknown = 0;
        public const int NumRows = 9;
        public const int NumCols = 9;
        public const int TotalPositions = NumRows * NumCols;

        private const int SingleSquareNumCols = 3;
        private const int SingleSquareNumRows = 3;
        private const string BoardLineSeparator = "+---+---+---+";
        private const string ColumnSeparator = "|";

        private int[][] Board { get; } = new int[NumRows][].SetAll(Unknown);

        public override string ToString()
        {
            var result = "";
            for (var row = 0; row < NumRows; row++)
            {
                if (row % SingleSquareNumRows == 0) result += BoardLineSeparator + Environment.NewLine;
                result += ToRowString(Board[row]) + Environment.NewLine;
            }

            result += BoardLineSeparator;
            return result;
        }

        public void SetValueAt(int row, int col, int digitValue)
        {
            Board[row][col] = digitValue;
        }

        public string DisplayBoard()
        {
            return string.Join(string.Empty,
                Board.Select(rowDigits =>
                {
                    return string.Join(string.Empty, rowDigits.Select(digit => digit == Unknown ? 0 : digit));
                }));
        }

        public int[] GetBoardAsNumber()
        {
            return Board.SelectMany(rowDigits => rowDigits).ToArray();
        }

        private string ToRowString(int[] rowDigits)
        {
            var result = "";
            for (var index = 0; index < rowDigits.Length; index++)
            {
                if (index % SingleSquareNumCols == 0) result += ColumnSeparator;
                result += rowDigits[index] == Unknown ? "." : rowDigits[index].ToString();
            }

            result += ColumnSeparator;
            return result;
        }

        public static SudokuBoard FromNumbers(int[] state)
        {
            var result = new SudokuBoard();
            for (var row = 0; row < NumRows; row++)
            for (var column = 0; column < NumCols; column++)
                result.SetValueAt(row, column, state[row * NumCols + column]);
            return result;
        }
    }
}