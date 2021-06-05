using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    public class ViableMove
    {
        public ViableMove(int rowToWrite, int colToWrite, bool[] usedDigits, int[] currentState, int currentStateIndex, int movedToDigit)
        {
            RowToWrite = rowToWrite;
            ColToWrite = colToWrite;
            UsedDigits = usedDigits;
            CurrentState = currentState;
            CurrentStateIndex = currentStateIndex;
            MovedToDigit = movedToDigit;
        }

        public int RowToWrite { get; private set; }
        public int ColToWrite { get; private set; }
        public bool[] UsedDigits { get; private set; }
        public int[] CurrentState { get; private set; }
        public int CurrentStateIndex { get; private set; }
        public int MovedToDigit { get; private set; }
    }

    public class SudokuBoardAndStackState
    {
        public SudokuBoardAndStackState()
        {
            // Construct board to be solved

            // Top element is current state of the board
            StateStack = new Stack<int[]>();

            // Prepare empty board
            SudokuBoard = new SudokuBoard();
        }

        public Stack<int[]> StateStack { get; }

        public SudokuBoard SudokuBoard { get; }

        public override string ToString()
        {
            return SudokuBoard.ToString();
        }

        public void ConstructFullyPopulatedBoardNonSense(Random randomNumbers)
        {
            #region Construct fully populated board

            // Top elements are (row, col) of cell which has been modified compared to previous state
            var rowIndexStack = new Stack<int>();
            var colIndexStack = new Stack<int>();

            // Top element indicates candidate digits (those with False) for (row, col)
            var usedDigitsStack = new Stack<bool[]>();

            // Top element is the value that was set on (row, col)
            var lastDigitStack = new Stack<int>();

            // Indicates operation to perform next
            // - expand - finds next empty cell and puts new state on stacks
            // - move - finds next candidate number at current pos and applies it to current state
            // - collapse - pops current state from stack as it did not yield a solution
            var command = Command.Expand;
            while (StateStack.Count <= 9 * 9)
                command = AppleSauce4(randomNumbers, command, rowIndexStack, colIndexStack,
                    usedDigitsStack,
                    lastDigitStack);

            #endregion
        }

        private Command AppleSauce4(Random randomNumbers,
            Command command,
            Stack<int> rowIndexStack,
            Stack<int> colIndexStack, Stack<bool[]> usedDigitsStack, Stack<int> lastDigitStack)
        {
            if (command.Equals(Command.Expand))
            {
                var currentState = new int[9 * 9];

                if (StateStack.Count > 0) Array.Copy(StateStack.Peek(), currentState, currentState.Length);

                var bestRow = -1;
                var bestCol = -1;
                bool[] bestUsedDigits = null;
                var bestCandidatesCount = -1;
                var bestRandomValue = -1;
                var containsUnsolvableCells = false;

                for (var index = 0; index < currentState.Length; index++)
                    if (currentState[index] == 0)
                    {
                        var row = index / 9;
                        var col = index % 9;
                        var blockRow = row / 3;
                        var blockCol = col / 3;

                        var isDigitUsed = new bool[9];

                        for (var i = 0; i < 9; i++)
                        {
                            var rowDigit = currentState[9 * i + col];
                            if (rowDigit > 0)
                                isDigitUsed[rowDigit - 1] = true;

                            var colDigit = currentState[9 * row + i];
                            if (colDigit > 0)
                                isDigitUsed[colDigit - 1] = true;

                            var blockDigit = currentState[(blockRow * 3 + i / 3) * 9 + blockCol * 3 + i % 3];
                            if (blockDigit > 0)
                                isDigitUsed[blockDigit - 1] = true;
                        }

                        var candidatesCount = isDigitUsed.Where(used => !used).Count();

                        if (candidatesCount == 0)
                        {
                            containsUnsolvableCells = true;
                            break;
                        }

                        var randomValue = randomNumbers.Next();

                        if (bestCandidatesCount < 0 ||
                            candidatesCount < bestCandidatesCount ||
                            candidatesCount == bestCandidatesCount && randomValue < bestRandomValue)
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
                return Command.Move;
            }

            if (command.Equals(Command.Collapse))
            {
                StateStack.Pop();
                rowIndexStack.Pop();
                colIndexStack.Pop();
                usedDigitsStack.Pop();
                lastDigitStack.Pop();

                return Command.Move;
            }

            if (command.Equals(Command.Move))
            {
                var viableMove = GetViableMove(rowIndexStack, colIndexStack, usedDigitsStack, lastDigitStack);

                if (viableMove != null)
                {
                    lastDigitStack.Push(viableMove.MovedToDigit);
                    viableMove.UsedDigits[viableMove.MovedToDigit - 1] = true;
                    viableMove.CurrentState[viableMove.CurrentStateIndex] = viableMove.MovedToDigit;
                    SudokuBoard.SetElementAt(viableMove.RowToWrite, viableMove.ColToWrite, viableMove.MovedToDigit);

                    // Next possible digit was found at current position
                    // Next step will be to expand the state
                    return Command.Expand;
                }

                // No viable candidate was found at current position - pop it in the next iteration
                lastDigitStack.Push(0);

                return Command.Collapse;
            }

            return command;
        }

        private ViableMove GetViableMove(Stack<int> rowIndexStack, Stack<int> colIndexStack, Stack<bool[]> usedDigitsStack, Stack<int> lastDigitStack)
        {
            var rowToMove = rowIndexStack.Peek();
            var colToMove = colIndexStack.Peek();
            var digitToMove = lastDigitStack.Pop();

            var rowToWrite = rowToMove + rowToMove / 3 + 1;
            var colToWrite = colToMove + colToMove / 3 + 1;

            var usedDigits = usedDigitsStack.Peek();
            var currentState = StateStack.Peek();
            var currentStateIndex = 9 * rowToMove + colToMove;

            var movedToDigit = digitToMove + 1;
            while (movedToDigit <= 9 && usedDigits[movedToDigit - 1])
                movedToDigit += 1;

            if (digitToMove > 0)
            {
                usedDigits[digitToMove - 1] = false;
                currentState[currentStateIndex] = 0;
                SudokuBoard.SetElementAt(rowToWrite, colToWrite, SudokuBoard.UNKNOWN);
            }

            if (movedToDigit <= 9)
            {
                return new ViableMove(rowToWrite, colToWrite, usedDigits, currentState, currentStateIndex, movedToDigit);    
            }

            return null;
        }
    }
}