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

        public char[][] Board { get; private set; }

        public char[][] GetEmptyBoard()
        {
            string line = "+---+---+---+";
            string middle = "|...|...|...|";
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

        public void SetElementAt(int row, int col, char value)
        {
            Board[row][col] = value;
        }
    }
}