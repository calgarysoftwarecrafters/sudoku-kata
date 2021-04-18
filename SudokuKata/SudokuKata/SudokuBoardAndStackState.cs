using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    public class SudokuBoardAndStackState
    {
        public SudokuBoardAndStackState(Stack<int[]> stateStack, char[][] board)
        {
            StateStack = stateStack;
            Board = board;
        }

        public Stack<int[]> StateStack { get; private set; }
        public char[][] Board { get; private set; }

        public string ToString(char[][] board)
        {
            return string.Join(Environment.NewLine, board.Select(s => new string(s)).ToArray());
        }
    }
}