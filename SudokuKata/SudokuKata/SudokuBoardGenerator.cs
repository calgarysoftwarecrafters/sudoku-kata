using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    public class SudokuBoardGenerator
    {
        public SudokuBoardGenerator()
        {
            // Construct board to be solved
            // Prepare empty board
            SudokuBoard = new SudokuBoard();
        }

        public SudokuBoard SudokuBoard { get; }

        public override string ToString()
        {
            return SudokuBoard.ToString();
        }

        public void ConstructPartiallySolvedBoard(Random randomNumbers)
        {
            #region Construct fully populated board

            var stacks = new Stacks();
            
            // Indicates operation to perform next
            // - expand - finds next empty cell and puts new state on stacks
            // - move - finds next candidate number at current pos and applies it to current state
            // - collapse - pops current state from stack as it did not yield a solution
            var command = Command.Expand;
            while (stacks.StateStack.Count <= 9 * 9)
            {
                
                command = PopulateBoardAndNextCommand(randomNumbers, command, stacks);
            }

            #endregion
        }

        public int[] GetBoardAsNumber()
        {
            return SudokuBoard.GetState();
        }

        private Command PopulateBoardAndNextCommand(Random randomNumbers,
            Command command, Stacks stacks)
        {
            if (command.Equals(Command.Expand))
            {
                return ExecuteExpandCommand(randomNumbers, stacks);
            }

            if (command.Equals(Command.Collapse))
            {
                return ExecuteCollapseCommand(stacks);
            }

            if (command.Equals(Command.Move))
            {
                return ExecuteMoveCommand(stacks);
            }

            return command;
        }

        private Command ExecuteExpandCommand(Random randomNumbers, Stacks stacks)
        {
            var currentState = new int[9 * 9];

            if (stacks.StateStack.Count > 0) Array.Copy(stacks.StateStack.Peek(), currentState, currentState.Length);

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
                stacks.StateStack.Push(currentState);
                stacks.RowIndexStack.Push(bestRow);
                stacks.ColIndexStack.Push(bestCol);
                stacks.UsedDigitsStack.Push(bestUsedDigits);
                stacks.LastDigitStack.Push(0); // No digit was tried at this position
            }

            // Always try to move after expand
            return Command.Move;
        }

        private Command ExecuteMoveCommand(Stacks stacks)
        {
            var viableMove = GetViableMove(stacks);

            if (viableMove != null)
            {
                stacks.LastDigitStack.Push(viableMove.MovedToDigit);
                viableMove.UsedDigits[viableMove.MovedToDigit - 1] = true;
                viableMove.CurrentState[viableMove.CurrentStateIndex] = viableMove.MovedToDigit;
                SudokuBoard.SetValueAt(viableMove.RowToWrite, viableMove.ColToWrite, viableMove.MovedToDigit);

                // Next possible digit was found at current position
                // Next step will be to expand the state
                return Command.Expand;
            }

            // No viable candidate was found at current position - pop it in the next iteration
            stacks.LastDigitStack.Push(0);

            return Command.Collapse;
        }

        private Command ExecuteCollapseCommand(Stacks stacks)
        {
            stacks.StateStack.Pop();
            stacks.RowIndexStack.Pop();
            stacks.ColIndexStack.Pop();
            stacks.UsedDigitsStack.Pop();
            stacks.LastDigitStack.Pop();

            return Command.Move;
        }

        private ViableMove GetViableMove(Stacks stacks)
        {
            var rowToMove = stacks.RowIndexStack.Peek();
            var colIndexStackTemp = stacks.ColIndexStack;
            var colToMove = colIndexStackTemp.Peek();
            var digitToMove = stacks.LastDigitStack.Pop();

            var usedDigits = stacks.UsedDigitsStack.Peek();
            var currentState = stacks.StateStack.Peek();
            var currentStateIndex = 9 * rowToMove + colToMove;

            var movedToDigit = digitToMove + 1;
            while (movedToDigit <= 9 && usedDigits[movedToDigit - 1])
                movedToDigit += 1;

            if (digitToMove > 0)
            {
                usedDigits[digitToMove - 1] = false;
                currentState[currentStateIndex] = 0;
                SudokuBoard.SetValueAt(rowToMove, colToMove, SudokuBoard.Unknown);
            }

            if (movedToDigit <= 9)
            {
                return new ViableMove(rowToMove, colToMove, usedDigits, currentState, currentStateIndex, movedToDigit);    
            }

            return null;
        }
    }
}