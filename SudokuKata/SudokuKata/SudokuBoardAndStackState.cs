using System.Collections.Generic;

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
    }
}