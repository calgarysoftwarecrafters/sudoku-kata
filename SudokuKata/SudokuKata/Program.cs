﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuKata
{
    public class Program
    {
        public static void Play(Random randomNumbers)
        {
            var partiallySolvedBoard = new SudokuBoardGenerator().ConstructPartiallySolvedBoard(randomNumbers);

            DisplayFinalLookOfTheSolvedBoard(partiallySolvedBoard);

            var puzzle = GeneratePuzzleFromPartiallySolvedBoard(randomNumbers,
                out var finalState, partiallySolvedBoard);

            Console.WriteLine();
            Console.WriteLine("Starting look of the board to solve:");
            Console.WriteLine(puzzle.ToString());
            
            var maskToOnesCount = PrepareLookupStructures(out var singleBitToIndex, out var allOnes);

            SolvePuzzle(randomNumbers, puzzle.GetBoardAsNumber(), allOnes, maskToOnesCount, singleBitToIndex, puzzle,
                finalState);
        }

        private static void DisplayFinalLookOfTheSolvedBoard(SudokuBoard sudokuBoard)
        {
            Console.WriteLine();
            Console.WriteLine("Final look of the solved board:");
            Console.WriteLine(sudokuBoard.ToString());
        }

        private static void SolvePuzzle(Random randomNumbers, int[] boardAsNumbers, int allOnes,
            Dictionary<int, int> maskToOnesCount,
            Dictionary<int, int> singleBitToIndex, SudokuBoard sudokuBoard, int[] finalState)
        {
            var wasChangeMade = true;
            while (wasChangeMade)
            {
                wasChangeMade = false;

                var candidateMasks = CalculateCandidatesForCurrentStateOfTheBoard(boardAsNumbers, allOnes);

                var cellGroups = BuildCellGroupsThatMapsCellIndicesToDistinctGroups(boardAsNumbers);

                var stepChangeMade = true;
                while (stepChangeMade)
                {
                    stepChangeMade = false;

                    wasChangeMade = PickCellsWithOnlyOneCandidateLeft(randomNumbers, candidateMasks, maskToOnesCount,
                        singleBitToIndex, boardAsNumbers, sudokuBoard, wasChangeMade);

                    wasChangeMade = FindANumberCanOnlyAppearInOnePlaceInRowColumnBlock(randomNumbers, wasChangeMade,
                        candidateMasks, boardAsNumbers, sudokuBoard);

                    stepChangeMade = RemovePairsOfDigitsInSameRowColumnBlocksFromOtherCollidingCells(wasChangeMade,
                        candidateMasks, maskToOnesCount, cellGroups, stepChangeMade);

                    stepChangeMade = TryToFindGroupsOfDigitsOfSizeN(wasChangeMade, stepChangeMade, maskToOnesCount,
                        cellGroups, boardAsNumbers, candidateMasks);
                }

                wasChangeMade = LookIfBoardHasMultipleSolutions(randomNumbers, wasChangeMade, candidateMasks,
                    maskToOnesCount,
                    finalState, boardAsNumbers, sudokuBoard);

                PrintBoardChange(wasChangeMade, sudokuBoard);
            }
        }

        private static Dictionary<int, int> PrepareLookupStructures(out Dictionary<int, int> singleBitToIndex,
            out int allOnes)
        {
            #region Prepare lookup structures that will be used in further execution

            Console.WriteLine();
            Console.WriteLine(new string('=', 80));
            Console.WriteLine();

            var maskToOnesCount = new Dictionary<int, int>();
            maskToOnesCount[0] = 0;
            for (var i = 1; i < 1 << 9; i++)
            {
                var smaller = i >> 1;
                var increment = i & 1;
                maskToOnesCount[i] = maskToOnesCount[smaller] + increment;
            }

            singleBitToIndex = new Dictionary<int, int>();
            for (var i = 0; i < 9; i++)
                singleBitToIndex[1 << i] = i;

            allOnes = (1 << 9) - 1;

            #endregion

            return maskToOnesCount;
        }

        private static int[] CalculateCandidatesForCurrentStateOfTheBoard(int[] state, int allOnes)
        {
            #region Calculate candidates for current state of the board

            var candidateMasks = new int[state.Length];

            for (var i = 0; i < state.Length; i++)
                if (state[i] == 0)
                {
                    var row = i / 9;
                    var col = i % 9;
                    var blockRow = row / 3;
                    var blockCol = col / 3;

                    var colidingNumbers = 0;
                    for (var j = 0; j < 9; j++)
                    {
                        var rowSiblingIndex = 9 * row + j;
                        var colSiblingIndex = 9 * j + col;
                        var blockSiblingIndex = 9 * (blockRow * 3 + j / 3) + blockCol * 3 + j % 3;

                        var rowSiblingMask = 1 << (state[rowSiblingIndex] - 1);
                        var colSiblingMask = 1 << (state[colSiblingIndex] - 1);
                        var blockSiblingMask = 1 << (state[blockSiblingIndex] - 1);

                        colidingNumbers = colidingNumbers | rowSiblingMask | colSiblingMask | blockSiblingMask;
                    }

                    candidateMasks[i] = allOnes & ~colidingNumbers;
                }

            #endregion

            return candidateMasks;
        }

        private static List<IGrouping<int, AppleSauce1>> BuildCellGroupsThatMapsCellIndicesToDistinctGroups(int[] state)
        {
            #region Build a collection (named cellGroups) which maps cell indices into distinct groups (rows/columns/blocks)

            var rowsIndices = state
                .Select((value, index) =>
                    new AppleSauce1(index / 9, $"row #{index / 9 + 1}", index, index / 9, index % 9))
                .GroupBy(tuple => tuple.Discriminator);

            var columnIndices = state
                .Select((value, index) =>
                    new AppleSauce1(9 + index % 9, $"column #{index % 9 + 1}", index, index / 9, index % 9))
                .GroupBy(tuple => tuple.Discriminator);

            var blockIndices = state
                .Select((value, index) => new
                {
                    Row = index / 9,
                    Column = index % 9,
                    Index = index
                })
                .Select(tuple => new AppleSauce1(18 + 3 * (tuple.Row / 3) + tuple.Column / 3,
                    $"block ({tuple.Row / 3 + 1}, {tuple.Column / 3 + 1})", tuple.Index, tuple.Row, tuple.Column))
                .GroupBy(tuple => tuple.Discriminator);

            var cellGroups =
                rowsIndices.Concat(columnIndices).Concat(blockIndices).ToList();

            #endregion

            return cellGroups;
        }

        private static bool PickCellsWithOnlyOneCandidateLeft(Random randomNumbers, int[] candidateMasks,
            Dictionary<int, int> maskToOnesCount, Dictionary<int, int> singleBitToIndex, int[] state,
            SudokuBoard sudokuBoard,
            bool changeMade)
        {
            #region Pick cells with only one candidate left

            var singleCandidateIndices =
                candidateMasks
                    .Select((mask, index) => new
                    {
                        CandidatesCount = maskToOnesCount[mask],
                        Index = index
                    })
                    .Where(tuple => tuple.CandidatesCount == 1)
                    .Select(tuple => tuple.Index)
                    .ToArray();

            if (singleCandidateIndices.Length > 0)
            {
                var pickSingleCandidateIndex = randomNumbers.Next(singleCandidateIndices.Length);
                var singleCandidateIndex = singleCandidateIndices[pickSingleCandidateIndex];
                var candidateMask = candidateMasks[singleCandidateIndex];
                var candidate = singleBitToIndex[candidateMask];

                var row = singleCandidateIndex / 9;
                var col = singleCandidateIndex % 9;

                state[singleCandidateIndex] = candidate + 1;
                sudokuBoard.SetValueAt(row, col, 1 + candidate);
                candidateMasks[singleCandidateIndex] = 0;
                changeMade = true;

                Console.WriteLine("({0}, {1}) can only contain {2}.", row + 1, col + 1, candidate + 1);
            }

            #endregion

            return changeMade;
        }

        private static bool FindANumberCanOnlyAppearInOnePlaceInRowColumnBlock(Random randomNumbers, bool changeMade,
            int[] candidateMasks, int[] state, SudokuBoard sudokuBoard)
        {
            #region Try to find a number which can only appear in one place in a row/column/block

            if (!changeMade)
            {
                var groupDescriptions = new List<string>();
                var candidateRowIndices = new List<int>();
                var candidateColIndices = new List<int>();
                var candidates = new List<int>();

                for (var digit = 1; digit <= 9; digit++)
                {
                    var mask = 1 << (digit - 1);
                    for (var cellGroup = 0; cellGroup < 9; cellGroup++)
                    {
                        var rowNumberCount = 0;
                        var indexInRow = 0;

                        var colNumberCount = 0;
                        var indexInCol = 0;

                        var blockNumberCount = 0;
                        var indexInBlock = 0;

                        for (var indexInGroup = 0; indexInGroup < 9; indexInGroup++)
                        {
                            var rowStateIndex = 9 * cellGroup + indexInGroup;
                            var colStateIndex = 9 * indexInGroup + cellGroup;
                            var blockRowIndex = cellGroup / 3 * 3 + indexInGroup / 3;
                            var blockColIndex = cellGroup % 3 * 3 + indexInGroup % 3;
                            var blockStateIndex = blockRowIndex * 9 + blockColIndex;

                            if ((candidateMasks[rowStateIndex] & mask) != 0)
                            {
                                rowNumberCount += 1;
                                indexInRow = indexInGroup;
                            }

                            if ((candidateMasks[colStateIndex] & mask) != 0)
                            {
                                colNumberCount += 1;
                                indexInCol = indexInGroup;
                            }

                            if ((candidateMasks[blockStateIndex] & mask) != 0)
                            {
                                blockNumberCount += 1;
                                indexInBlock = indexInGroup;
                            }
                        }

                        if (rowNumberCount == 1)
                        {
                            groupDescriptions.Add($"Row #{cellGroup + 1}");
                            candidateRowIndices.Add(cellGroup);
                            candidateColIndices.Add(indexInRow);
                            candidates.Add(digit);
                        }

                        if (colNumberCount == 1)
                        {
                            groupDescriptions.Add($"Column #{cellGroup + 1}");
                            candidateRowIndices.Add(indexInCol);
                            candidateColIndices.Add(cellGroup);
                            candidates.Add(digit);
                        }

                        if (blockNumberCount == 1)
                        {
                            var blockRow = cellGroup / 3;
                            var blockCol = cellGroup % 3;

                            groupDescriptions.Add($"Block ({blockRow + 1}, {blockCol + 1})");
                            candidateRowIndices.Add(blockRow * 3 + indexInBlock / 3);
                            candidateColIndices.Add(blockCol * 3 + indexInBlock % 3);
                            candidates.Add(digit);
                        }
                    }
                }

                if (candidates.Count > 0)
                {
                    var index = randomNumbers.Next(candidates.Count);
                    var description = groupDescriptions.ElementAt(index);
                    var row = candidateRowIndices.ElementAt(index);
                    var col = candidateColIndices.ElementAt(index);
                    var digit = candidates.ElementAt(index);

                    var message = $"{description} can contain {digit} only at ({row + 1}, {col + 1}).";

                    var stateIndex = 9 * row + col;
                    state[stateIndex] = digit;
                    candidateMasks[stateIndex] = 0;
                    sudokuBoard.SetValueAt(row, col, digit);

                    changeMade = true;

                    Console.WriteLine(message);
                }
            }

            #endregion

            return changeMade;
        }

        private static bool RemovePairsOfDigitsInSameRowColumnBlocksFromOtherCollidingCells(bool changeMade,
            int[] candidateMasks, Dictionary<int, int> maskToOnesCount, List<IGrouping<int, AppleSauce1>> cellGroups,
            bool stepChangeMade)
        {
            #region Try to find pairs of digits in the same row/column/block and remove them from other colliding cells

            if (!changeMade)
            {
                IEnumerable<int> twoDigitMasks =
                    candidateMasks.Where(mask => maskToOnesCount[mask] == 2).Distinct().ToList();

                var groups =
                    twoDigitMasks
                        .SelectMany(mask =>
                            cellGroups
                                .Where(group => group.Count(tuple => candidateMasks[tuple.Index] == mask) == 2)
                                .Where(group => group.Any(tuple =>
                                    candidateMasks[tuple.Index] != mask && (candidateMasks[tuple.Index] & mask) > 0))
                                .Select(group => new AppleSauce2(mask, group.Key, group.First().Description, group)))
                        .ToList();

                if (groups.Any())
                    foreach (var group in groups)
                    {
                        var cells =
                            group.Cells
                                .Where(
                                    cell =>
                                        candidateMasks[cell.Index] != group.Mask &&
                                        (candidateMasks[cell.Index] & group.Mask) > 0)
                                .ToList();

                        var maskCells =
                            group.Cells
                                .Where(cell => candidateMasks[cell.Index] == group.Mask)
                                .ToArray();


                        if (cells.Any())
                        {
                            var upper = 0;
                            var lower = 0;
                            var temp = group.Mask;

                            var value = 1;
                            while (temp > 0)
                            {
                                if ((temp & 1) > 0)
                                {
                                    lower = upper;
                                    upper = value;
                                }

                                temp = temp >> 1;
                                value += 1;
                            }

                            Console.WriteLine(
                                $"Values {lower} and {upper} in {group.Description} are in cells ({maskCells[0].Row + 1}, {maskCells[0].Column + 1}) and ({maskCells[1].Row + 1}, {maskCells[1].Column + 1}).");

                            foreach (var cell in cells)
                            {
                                var maskToRemove = candidateMasks[cell.Index] & group.Mask;
                                var valuesToRemove = new List<int>();
                                var curValue = 1;
                                while (maskToRemove > 0)
                                {
                                    if ((maskToRemove & 1) > 0) valuesToRemove.Add(curValue);

                                    maskToRemove = maskToRemove >> 1;
                                    curValue += 1;
                                }

                                var valuesReport = string.Join(", ", valuesToRemove.ToArray());
                                Console.WriteLine(
                                    $"{valuesReport} cannot appear in ({cell.Row + 1}, {cell.Column + 1}).");

                                candidateMasks[cell.Index] &= ~group.Mask;
                                stepChangeMade = true;
                            }
                        }
                    }
            }

            #endregion

            return stepChangeMade;
        }

        private static SudokuBoard GeneratePuzzleFromPartiallySolvedBoard(Random randomNumbers,
            out int[] finalState, SudokuBoard sudokuBoard)
        {
            #region Generate inital board from the completely solved one

            // Board is solved at this point.
            // Now pick subset of digits as the starting position.
            var remainingDigits = 30;
            var maxRemovedPerBlock = 6;
            var removedPerBlock = new int[3, 3];
            var positions = Enumerable.Range(0, 9 * 9).ToArray();
            var state = sudokuBoard.GetBoardAsNumber();

            finalState = new int[state.Length];
            Array.Copy(state, finalState, finalState.Length);

            var puzzle = SoySauce1(randomNumbers, remainingDigits, positions, removedPerBlock, maxRemovedPerBlock, state,
                sudokuBoard);

            #endregion

            return puzzle;
        }

        private static SudokuBoard SoySauce1(Random randomNumbers, int remainingDigits,
            int[] positions, int[,] removedPerBlock, int maxRemovedPerBlock, int[] state, SudokuBoard sudokuBoard)
        {
            var removedPos = 0;
            while (removedPos < 9 * 9 - remainingDigits)
            {
                var curRemainingDigits = positions.Length - removedPos;
                var indexToPick = removedPos + randomNumbers.Next(curRemainingDigits);

                var row = positions[indexToPick] / 9;
                var col = positions[indexToPick] % 9;

                var blockRowToRemove = row / 3;
                var blockColToRemove = col / 3;

                if (removedPerBlock[blockRowToRemove, blockColToRemove] >= maxRemovedPerBlock)
                    continue;

                removedPerBlock[blockRowToRemove, blockColToRemove] += 1;

                var temp = positions[removedPos];
                positions[removedPos] = positions[indexToPick];
                positions[indexToPick] = temp;

                sudokuBoard.SetValueAt(row, col, SudokuBoard.Unknown);

                var stateIndex = 9 * row + col;
                state[stateIndex] = 0;

                removedPos += 1;
            }
            
            return SudokuBoard.FromNumbers(state);
        }

        private static bool TryToFindGroupsOfDigitsOfSizeN(bool changeMade, bool stepChangeMade,
            Dictionary<int, int> maskToOnesCount,
            List<IGrouping<int, AppleSauce1>> cellGroups, int[] state, int[] candidateMasks)
        {
            #region Try to find groups of digits of size N which only appear in N cells within row/column/block

            // When a set of N digits only appears in N cells within row/column/block, then no other digit can appear in the same set of cells
            // All other candidates can then be removed from those cells

            if (!changeMade && !stepChangeMade)
            {
                IEnumerable<int> masks =
                    maskToOnesCount
                        .Where(tuple => tuple.Value > 1)
                        .Select(tuple => tuple.Key).ToList();

                var groupsWithNMasks =
                    masks
                        .SelectMany(mask =>
                            cellGroups
                                .Where(group => group.All(cell =>
                                    state[cell.Index] == 0 || (mask & (1 << (state[cell.Index] - 1))) == 0))
                                .Select(group => new AppleSauce3(mask, group.First().Description, group,
                                    group.Where(cell =>
                                            state[cell.Index] == 0 && (candidateMasks[cell.Index] & mask) != 0)
                                        .ToList(), group.Count(
                                        cell => state[cell.Index] == 0 &&
                                                (candidateMasks[cell.Index] & mask) != 0 &&
                                                (candidateMasks[cell.Index] & ~mask) != 0))))
                        .Where(group => group.CellsWithMask.Count() == maskToOnesCount[group.Mask])
                        .ToList();

                foreach (var groupWithNMasks in groupsWithNMasks)
                {
                    var mask = groupWithNMasks.Mask;

                    if (groupWithNMasks.Cells
                        .Any(cell =>
                            (candidateMasks[cell.Index] & mask) != 0 &&
                            (candidateMasks[cell.Index] & ~mask) != 0))
                    {
                        var message = new StringBuilder();
                        message.Append($"In {groupWithNMasks.Description} values ");

                        var separator = string.Empty;
                        var temp = mask;
                        var curValue = 1;
                        while (temp > 0)
                        {
                            if ((temp & 1) > 0)
                            {
                                message.Append($"{separator}{curValue}");
                                separator = ", ";
                            }

                            temp = temp >> 1;
                            curValue += 1;
                        }

                        message.Append(" appear only in cells");
                        foreach (var cell in groupWithNMasks.CellsWithMask)
                            message.Append($" ({cell.Row + 1}, {cell.Column + 1})");

                        message.Append(" and other values cannot appear in those cells.");

                        Console.WriteLine(message.ToString());
                    }

                    foreach (var cell in groupWithNMasks.CellsWithMask)
                    {
                        var maskToClear = candidateMasks[cell.Index] & ~groupWithNMasks.Mask;
                        if (maskToClear == 0)
                            continue;

                        candidateMasks[cell.Index] &= groupWithNMasks.Mask;
                        stepChangeMade = true;

                        var valueToClear = 1;

                        var separator = string.Empty;
                        var message = new StringBuilder();

                        while (maskToClear > 0)
                        {
                            if ((maskToClear & 1) > 0)
                            {
                                message.Append($"{separator}{valueToClear}");
                                separator = ", ";
                            }

                            maskToClear = maskToClear >> 1;
                            valueToClear += 1;
                        }

                        message.Append($" cannot appear in cell ({cell.Row + 1}, {cell.Column + 1}).");
                        Console.WriteLine(message.ToString());
                    }
                }
            }

            #endregion

            return stepChangeMade;
        }

        private static bool LookIfBoardHasMultipleSolutions(Random randomNumbers, bool changeMade, int[] candidateMasks,
            Dictionary<int, int> maskToOnesCount, int[] finalState, int[] state,
            SudokuBoard sudokuBoard)
        {
            Stack<int[]> stateStack;
            Stack<int> rowIndexStack;
            Stack<int> colIndexStack;
            Stack<bool[]> usedDigitsStack;
            Stack<int> lastDigitStack;

            #region Final attempt - look if the board has multiple solutions

            if (!changeMade)
            {
                // This is the last chance to do something in this iteration:
                // If this attempt fails, board will not be entirely solved.

                // Try to see if there are pairs of values that can be exchanged arbitrarily
                // This happens when board has more than one valid solution

                var candidateIndex1 = new Queue<int>();
                var candidateIndex2 = new Queue<int>();
                var candidateDigit1 = new Queue<int>();
                var candidateDigit2 = new Queue<int>();

                for (var i = 0; i < candidateMasks.Length - 1; i++)
                    if (maskToOnesCount[candidateMasks[i]] == 2)
                    {
                        var row = i / 9;
                        var col = i % 9;
                        var blockIndex = 3 * (row / 3) + col / 3;

                        var temp = candidateMasks[i];
                        var lower = 0;
                        var upper = 0;
                        for (var digit = 1; temp > 0; digit++)
                        {
                            if ((temp & 1) != 0)
                            {
                                lower = upper;
                                upper = digit;
                            }

                            temp = temp >> 1;
                        }

                        for (var j = i + 1; j < candidateMasks.Length; j++)
                            if (candidateMasks[j] == candidateMasks[i])
                            {
                                var row1 = j / 9;
                                var col1 = j % 9;
                                var blockIndex1 = 3 * (row1 / 3) + col1 / 3;

                                if (row == row1 || col == col1 || blockIndex == blockIndex1)
                                {
                                    candidateIndex1.Enqueue(i);
                                    candidateIndex2.Enqueue(j);
                                    candidateDigit1.Enqueue(lower);
                                    candidateDigit2.Enqueue(upper);
                                }
                            }
                    }

                // At this point we have the lists with pairs of cells that might pick one of two digits each
                // Now we have to check whether that is really true - does the board have two solutions?

                var stateIndex1 = new List<int>();
                var stateIndex2 = new List<int>();
                var value1 = new List<int>();
                var value2 = new List<int>();

                while (candidateIndex1.Any())
                {
                    var index1 = candidateIndex1.Dequeue();
                    var index2 = candidateIndex2.Dequeue();
                    var digit1 = candidateDigit1.Dequeue();
                    var digit2 = candidateDigit2.Dequeue();

                    var alternateState = new int[finalState.Length];
                    Array.Copy(state, alternateState, alternateState.Length);

                    if (finalState[index1] == digit1)
                    {
                        alternateState[index1] = digit2;
                        alternateState[index2] = digit1;
                    }
                    else
                    {
                        alternateState[index1] = digit1;
                        alternateState[index2] = digit2;
                    }

                    // What follows below is a complete copy-paste of the solver which appears at the beginning of this method
                    // However, the algorithm couldn't be applied directly and it had to be modified.
                    // Implementation below assumes that the board might not have a solution.
                    stateStack = new Stack<int[]>();
                    rowIndexStack = new Stack<int>();
                    colIndexStack = new Stack<int>();
                    usedDigitsStack = new Stack<bool[]>();
                    lastDigitStack = new Stack<int>();

                    //var command = Command.ExpandCommandName;
                    var commandObj = Command.Expand;
                    while (!commandObj.Equals(Command.Complete) && !commandObj.Equals(Command.Fail))
                        if (commandObj.Equals(Command.Expand))
                        {
                            var currentState = new int[9 * 9];

                            if (stateStack.Any())
                                Array.Copy(stateStack.Peek(), currentState, currentState.Length);
                            else
                                Array.Copy(alternateState, currentState, currentState.Length);

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

                                        var blockDigit =
                                            currentState[(blockRow * 3 + i / 3) * 9 + blockCol * 3 + i % 3];
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
                                stateStack.Push(currentState);
                                rowIndexStack.Push(bestRow);
                                colIndexStack.Push(bestCol);
                                usedDigitsStack.Push(bestUsedDigits);
                                lastDigitStack.Push(0); // No digit was tried at this position
                            }

                            // Always try to move after expand
                            commandObj = Command.Move;
                        }
                        else if (commandObj.Equals(Command.Collapse))
                        {
                            stateStack.Pop();
                            rowIndexStack.Pop();
                            colIndexStack.Pop();
                            usedDigitsStack.Pop();
                            lastDigitStack.Pop();

                            if (stateStack.Any())
                                commandObj = Command.Move; // Always try to move after collapse
                            else
                                commandObj = Command.Fail;
                        }
                        else if (commandObj.Equals(Command.Move))
                        {
                            var rowToMove = rowIndexStack.Peek();
                            var colToMove = colIndexStack.Peek();
                            var digitToMove = lastDigitStack.Pop();

                            var usedDigits = usedDigitsStack.Peek();
                            var currentState = stateStack.Peek();
                            var currentStateIndex = 9 * rowToMove + colToMove;

                            var movedToDigit = digitToMove + 1;
                            while (movedToDigit <= 9 && usedDigits[movedToDigit - 1])
                                movedToDigit += 1;

                            if (digitToMove > 0)
                            {
                                usedDigits[digitToMove - 1] = false;
                                currentState[currentStateIndex] = 0;
                                sudokuBoard.SetValueAt(rowToMove, colToMove, SudokuBoard.Unknown);
                            }

                            if (movedToDigit <= 9)
                            {
                                lastDigitStack.Push(movedToDigit);
                                usedDigits[movedToDigit - 1] = true;
                                currentState[currentStateIndex] = movedToDigit;
                                sudokuBoard.SetValueAt(rowToMove, colToMove, movedToDigit);

                                if (currentState.Any(digit => digit == 0))
                                    commandObj = Command.Expand;
                                else
                                    commandObj = Command.Complete;
                            }
                            else
                            {
                                // No viable candidate was found at current position - pop it in the next iteration
                                lastDigitStack.Push(0);
                                commandObj = Command.Collapse;
                            }
                        }

                    if (commandObj.Equals(Command.Complete))
                    {
                        // Board was solved successfully even with two digits swapped
                        stateIndex1.Add(index1);
                        stateIndex2.Add(index2);
                        value1.Add(digit1);
                        value2.Add(digit2);
                    }
                }

                if (stateIndex1.Any())
                {
                    var pos = randomNumbers.Next(stateIndex1.Count());
                    var index1 = stateIndex1.ElementAt(pos);
                    var index2 = stateIndex2.ElementAt(pos);
                    var digit1 = value1.ElementAt(pos);
                    var digit2 = value2.ElementAt(pos);
                    var row1 = index1 / 9;
                    var col1 = index1 % 9;
                    var row2 = index2 / 9;
                    var col2 = index2 % 9;

                    var description = string.Empty;

                    if (index1 / 9 == index2 / 9)
                        description = $"row #{index1 / 9 + 1}";
                    else if (index1 % 9 == index2 % 9)
                        description = $"column #{index1 % 9 + 1}";
                    else
                        description = $"block ({row1 / 3 + 1}, {col1 / 3 + 1})";

                    state[index1] = finalState[index1];
                    state[index2] = finalState[index2];
                    candidateMasks[index1] = 0;
                    candidateMasks[index2] = 0;
                    changeMade = true;

                    for (var i = 0; i < state.Length; i++)
                    {
                        var tempRow = i / 9;
                        var tempCol = i % 9;

                        sudokuBoard.SetValueAt(tempRow, tempCol, SudokuBoard.Unknown);
                        if (state[i] > 0) sudokuBoard.SetValueAt(tempRow, tempCol, state[i]);
                    }

                    Console.WriteLine(
                        $"Guessing that {digit1} and {digit2} are arbitrary in {description} (multiple solutions): Pick {finalState[index1]}->({row1 + 1}, {col1 + 1}), {finalState[index2]}->({row2 + 1}, {col2 + 1}).");
                }
            }

            #endregion

            return changeMade;
        }

        private static void PrintBoardChange(bool changeMade, SudokuBoard sudokuBoard)
        {
            if (changeMade)
            {
                #region Print the board as it looks after one change was made to it

                Console.WriteLine(sudokuBoard.ToString());
                Console.WriteLine("Code: {0}", sudokuBoard.DisplayBoard());
                Console.WriteLine();

                #endregion
            }
        }

        private static void Main(string[] args)
        {
            Play(new Random());

            Console.WriteLine();
            Console.Write("Press ENTER to exit... ");
            Console.ReadLine();
        }
    }
}