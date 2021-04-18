using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    public class SudokuBoardAndStackState
    {
        public SudokuBoardAndStackState()
        {
            // Construct board to be solved

            // Top element is current state of the board
            StateStack = new Stack<int[]>();

            // Prepare empty board
            Board = GetEmptyBoard();
        }

        private char[][] GetEmptyBoard()
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

        public Stack<int[]> StateStack { get; private set; }
        public char[][] Board { get; private set; }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Board.Select(s => new string(s)).ToArray());
        }

        public void ConstructFullyPopulatedBoardNonSense(Random randomNumbers)
        {
            #region Construct fully populated board

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
            while (StateStack.Count <= 9 * 9)
            {
                command = AppleSauce4(randomNumbers, command, rowIndexStack, colIndexStack,
                    usedDigitsStack,
                    lastDigitStack);
            }

            #endregion
        }

        public string AppleSauce4(Random randomNumbers,
            string command,
            Stack<int> rowIndexStack,
            Stack<int> colIndexStack, Stack<bool[]> usedDigitsStack, Stack<int> lastDigitStack)
        {
            if (command == "expand")
            {
                int[] currentState = new int[9 * 9];

                if (StateStack.Count > 0)
                {
                    Array.Copy(StateStack.Peek(), currentState, currentState.Length);
                }

                int bestRow = -1;
                int bestCol = -1;
                bool[] bestUsedDigits = null;
                int bestCandidatesCount = -1;
                int bestRandomValue = -1;
                bool containsUnsolvableCells = false;

                for (int index = 0; index < currentState.Length; index++)
                    if (currentState[index] == 0)
                    {
                        int row = index / 9;
                        int col = index % 9;
                        int blockRow = row / 3;
                        int blockCol = col / 3;

                        bool[] isDigitUsed = new bool[9];

                        for (int i = 0; i < 9; i++)
                        {
                            int rowDigit = currentState[9 * i + col];
                            if (rowDigit > 0)
                                isDigitUsed[rowDigit - 1] = true;

                            int colDigit = currentState[9 * row + i];
                            if (colDigit > 0)
                                isDigitUsed[colDigit - 1] = true;

                            int blockDigit = currentState[(blockRow * 3 + i / 3) * 9 + (blockCol * 3 + i % 3)];
                            if (blockDigit > 0)
                                isDigitUsed[blockDigit - 1] = true;
                        }

                        int candidatesCount = isDigitUsed.Where(used => !used).Count();

                        if (candidatesCount == 0)
                        {
                            containsUnsolvableCells = true;
                            break;
                        }

                        int randomValue = randomNumbers.Next();

                        if (bestCandidatesCount < 0 ||
                            candidatesCount < bestCandidatesCount ||
                            (candidatesCount == bestCandidatesCount && randomValue < bestRandomValue))
                        {
                            bestRow = row;
                            bestCol = col;
                            bestUsedDigits = isDigitUsed;
                            bestCandidatesCount = candidatesCount;
                            bestRandomValue = randomValue;
                        }
                    }

                if (!containsUnsolvableCells)
                {
                    StateStack.Push(currentState);
                    rowIndexStack.Push(bestRow);
                    colIndexStack.Push(bestCol);
                    usedDigitsStack.Push(bestUsedDigits);
                    lastDigitStack.Push(0); // No digit was tried at this position
                }

                // Always try to move after expand
                command = "move";
            }
            else if (command == "collapse")
            {
                StateStack.Pop();
                rowIndexStack.Pop();
                colIndexStack.Pop();
                usedDigitsStack.Pop();
                lastDigitStack.Pop();

                command = "move"; // Always try to move after collapse
            }
            else if (command == "move")
            {
                int rowToMove = rowIndexStack.Peek();
                int colToMove = colIndexStack.Peek();
                int digitToMove = lastDigitStack.Pop();

                int rowToWrite = rowToMove + rowToMove / 3 + 1;
                int colToWrite = colToMove + colToMove / 3 + 1;

                bool[] usedDigits = usedDigitsStack.Peek();
                int[] currentState = StateStack.Peek();
                int currentStateIndex = 9 * rowToMove + colToMove;

                int movedToDigit = digitToMove + 1;
                while (movedToDigit <= 9 && usedDigits[movedToDigit - 1])
                    movedToDigit += 1;

                if (digitToMove > 0)
                {
                    usedDigits[digitToMove - 1] = false;
                    currentState[currentStateIndex] = 0;
                    Board[rowToWrite][colToWrite] = '.';
                }

                if (movedToDigit <= 9)
                {
                    lastDigitStack.Push(movedToDigit);
                    usedDigits[movedToDigit - 1] = true;
                    currentState[currentStateIndex] = movedToDigit;
                    Board[rowToWrite][colToWrite] = (char) ('0' + movedToDigit);

                    // Next possible digit was found at current position
                    // Next step will be to expand the state
                    command = "expand";
                }
                else
                {
                    // No viable candidate was found at current position - pop it in the next iteration
                    lastDigitStack.Push(0);
                    command = "collapse";
                }
            }

            return command;
        }
    }
}