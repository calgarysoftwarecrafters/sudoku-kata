using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    public class SudokuBoardAndStackState
    {
        public SudokuBoardAndStackState()
        {
            // Prepare empty board
            string line = "+---+---+---+";
            string middle = "|...|...|...|";

            // Construct board to be solved

            // Top element is current state of the board
            StateStack = new Stack<int[]>();
            Board = new[]
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

        public Stack<int[]> StateStack { get; private set; }
        public char[][] Board { get; private set; }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Board.Select(s => new string(s)).ToArray());
        }
    }
}