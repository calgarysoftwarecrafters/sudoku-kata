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

        public static SudokuBoardAndStackState ConstructFullyPopulatedBoard(Random randomNumbers)
        {
            #region Construct fully populated board

            var sudokuBoardAndStackState = new SudokuBoardAndStackState();

            // Top elements are (row, col) of cell which has been modified compared to previous state
            Stack<int> rowIndexStack = new Stack<int>();
            Stack<int> colIndexStack = new Stack<int>();

            // Top element indicates candidate digits (those with False) for (row, col)
            Stack<bool[]> usedDigitsStack = new Stack<bool[]>();

            // Top element is the value that was set on (row, col)
            Stack<int> lastDigitStack = new Stack<int>();

            // Indicates operation to perform next
            // - expand - finds next empty cell and puts new state on stacks
            // - move - finds next candidate number at current pos and applies it to current state
            // - collapse - pops current state from stack as it did not yield a solution
            string command = "expand";
            while (sudokuBoardAndStackState.StateStack.Count <= 9 * 9)
            {
                command = Program.AppleSauce4(randomNumbers, sudokuBoardAndStackState, command, rowIndexStack, colIndexStack, usedDigitsStack,
                    lastDigitStack);
            }

            Console.WriteLine();
            Console.WriteLine("Final look of the solved board:");
            var boardString = sudokuBoardAndStackState.ToString();
            Console.WriteLine(boardString);

            #endregion

            return sudokuBoardAndStackState;
        }
    }
}