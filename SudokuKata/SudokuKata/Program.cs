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
            var sudokuBoardAndStackState = new SudokuBoardAndStackState();

            sudokuBoardAndStackState.ConstructFullyPopulatedBoardNonSense(randomNumbers);

            DisplayFinalLookOfTheSolvedBoard(sudokuBoardAndStackState);

            var state = GenerateInitialBoardFromCompletelySolvedOne(randomNumbers,
                out var finalState, sudokuBoardAndStackState);

            var maskToOnesCount = PrepareLookupStructures(out var singleBitToIndex, out var allOnes);

            AppleSauce5(randomNumbers, state, allOnes, maskToOnesCount, singleBitToIndex, sudokuBoardAndStackState.SudokuBoard.Board, finalState);
        }

        private static void DisplayFinalLookOfTheSolvedBoard(SudokuBoardAndStackState sudokuBoardAndStackState)
        {
            Console.WriteLine();
            Console.WriteLine("Final look of the solved board:");
            var boardString = sudokuBoardAndStackState.SudokuBoard.ToString();
            Console.WriteLine(boardString);
        }

        private static void AppleSauce5(Random randomNumbers, int[] state, int allOnes, Dictionary<int, int> maskToOnesCount,
            Dictionary<int, int> singleBitToIndex, char[][] board, int[] finalState)
        {
            bool changeMade = true;
            while (changeMade)
            {
                changeMade = false;

                var candidateMasks = CalculateCandidatesForCurrentStateOfTheBoard(state, allOnes);

                var cellGroups = BuildCellGroupsThatMapsCellIndicesToDistinctGroups(state);

                bool stepChangeMade = true;
                while (stepChangeMade)
                {
                    stepChangeMade = false;

                    changeMade = PickCellsWithOnlyOneCandidateLeft(randomNumbers, candidateMasks, maskToOnesCount,
                        singleBitToIndex, state, board, changeMade);

                    changeMade = FindANumberCanOnlyAppearInOnePlaceInRowColumnBlock(randomNumbers, changeMade,
                        candidateMasks, state, board);

                    stepChangeMade = RemovePairsOfDigitsInSameRowColumnBlocksFromOtherCollidingCells(changeMade,
                        candidateMasks, maskToOnesCount, cellGroups, stepChangeMade);

                    stepChangeMade = TryToFindGroupsOfDigitsOfSizeN(changeMade, stepChangeMade, maskToOnesCount,
                        cellGroups, state, candidateMasks);
                }

                changeMade = LookIfBoardHasMultipleSolutions(randomNumbers, changeMade, candidateMasks, maskToOnesCount,
                    finalState, state, board);

                PrintBoardChange(changeMade, board);
            }
        }

        private static Dictionary<int, int> PrepareLookupStructures(out Dictionary<int, int> singleBitToIndex,
            out int allOnes)
        {
            #region Prepare lookup structures that will be used in further execution

            Console.WriteLine();
            Console.WriteLine(new string('=', 80));
            Console.WriteLine();

            Dictionary<int, int> maskToOnesCount = new Dictionary<int, int>();
            maskToOnesCount[0] = 0;
            for (int i = 1; i < (1 << 9); i++)
            {
                int smaller = i >> 1;
                int increment = i & 1;
                maskToOnesCount[i] = maskToOnesCount[smaller] + increment;
            }

            singleBitToIndex = new Dictionary<int, int>();
            for (int i = 0; i < 9; i++)
                singleBitToIndex[1 << i] = i;

            allOnes = (1 << 9) - 1;

            #endregion

            return maskToOnesCount;
        }

        private static int[] CalculateCandidatesForCurrentStateOfTheBoard(int[] state, int allOnes)
        {
            #region Calculate candidates for current state of the board

            int[] candidateMasks = new int[state.Length];

            for (int i = 0; i < state.Length; i++)
                if (state[i] == 0)
                {
                    int row = i / 9;
                    int col = i % 9;
                    int blockRow = row / 3;
                    int blockCol = col / 3;

                    int colidingNumbers = 0;
                    for (int j = 0; j < 9; j++)
                    {
                        int rowSiblingIndex = 9 * row + j;
                        int colSiblingIndex = 9 * j + col;
                        int blockSiblingIndex = 9 * (blockRow * 3 + j / 3) + blockCol * 3 + j % 3;

                        int rowSiblingMask = 1 << (state[rowSiblingIndex] - 1);
                        int colSiblingMask = 1 << (state[colSiblingIndex] - 1);
                        int blockSiblingMask = 1 << (state[blockSiblingIndex] - 1);

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

            IEnumerable<IGrouping<int, AppleSauce1>> rowsIndices = state
                .Select((value, index) =>
                    new AppleSauce1(index / 9, $"row #{index / 9 + 1}", index, index / 9, index % 9))
                .GroupBy(tuple => tuple.Discriminator);

            IEnumerable<IGrouping<int, AppleSauce1>> columnIndices = state
                .Select((value, index) =>
                    new AppleSauce1(9 + index % 9, $"column #{index % 9 + 1}", index, index / 9, index % 9))
                .GroupBy(tuple => tuple.Discriminator);

            IEnumerable<IGrouping<int, AppleSauce1>> blockIndices = state
                .Select((value, index) => new
                {
                    Row = index / 9,
                    Column = index % 9,
                    Index = index
                })
                .Select(tuple => new AppleSauce1(18 + 3 * (tuple.Row / 3) + tuple.Column / 3,
                    $"block ({tuple.Row / 3 + 1}, {tuple.Column / 3 + 1})", tuple.Index, tuple.Row, tuple.Column))
                .GroupBy(tuple => tuple.Discriminator);

            List<IGrouping<int, AppleSauce1>> cellGroups =
                rowsIndices.Concat(columnIndices).Concat(blockIndices).ToList();

            #endregion

            return cellGroups;
        }

        private static bool PickCellsWithOnlyOneCandidateLeft(Random randomNumbers, int[] candidateMasks,
            Dictionary<int, int> maskToOnesCount, Dictionary<int, int> singleBitToIndex, int[] state, char[][] board,
            bool changeMade)
        {
            #region Pick cells with only one candidate left

            int[] singleCandidateIndices =
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
                int pickSingleCandidateIndex = randomNumbers.Next(singleCandidateIndices.Length);
                int singleCandidateIndex = singleCandidateIndices[pickSingleCandidateIndex];
                int candidateMask = candidateMasks[singleCandidateIndex];
                int candidate = singleBitToIndex[candidateMask];

                int row = singleCandidateIndex / 9;
                int col = singleCandidateIndex % 9;

                int rowToWrite = row + row / 3 + 1;
                int colToWrite = col + col / 3 + 1;

                state[singleCandidateIndex] = candidate + 1;
                board[rowToWrite][colToWrite] = (char) ('1' + candidate);
                candidateMasks[singleCandidateIndex] = 0;
                changeMade = true;

                Console.WriteLine("({0}, {1}) can only contain {2}.", row + 1, col + 1, candidate + 1);
            }

            #endregion

            return changeMade;
        }

        private static bool FindANumberCanOnlyAppearInOnePlaceInRowColumnBlock(Random randomNumbers, bool changeMade,
            int[] candidateMasks, int[] state, char[][] board)
        {
            #region Try to find a number which can only appear in one place in a row/column/block

            if (!changeMade)
            {
                List<string> groupDescriptions = new List<string>();
                List<int> candidateRowIndices = new List<int>();
                List<int> candidateColIndices = new List<int>();
                List<int> candidates = new List<int>();

                for (int digit = 1; digit <= 9; digit++)
                {
                    int mask = 1 << (digit - 1);
                    for (int cellGroup = 0; cellGroup < 9; cellGroup++)
                    {
                        int rowNumberCount = 0;
                        int indexInRow = 0;

                        int colNumberCount = 0;
                        int indexInCol = 0;

                        int blockNumberCount = 0;
                        int indexInBlock = 0;

                        for (int indexInGroup = 0; indexInGroup < 9; indexInGroup++)
                        {
                            int rowStateIndex = 9 * cellGroup + indexInGroup;
                            int colStateIndex = 9 * indexInGroup + cellGroup;
                            int blockRowIndex = (cellGroup / 3) * 3 + indexInGroup / 3;
                            int blockColIndex = (cellGroup % 3) * 3 + indexInGroup % 3;
                            int blockStateIndex = blockRowIndex * 9 + blockColIndex;

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
                            int blockRow = cellGroup / 3;
                            int blockCol = cellGroup % 3;

                            groupDescriptions.Add($"Block ({blockRow + 1}, {blockCol + 1})");
                            candidateRowIndices.Add(blockRow * 3 + indexInBlock / 3);
                            candidateColIndices.Add(blockCol * 3 + indexInBlock % 3);
                            candidates.Add(digit);
                        }
                    }
                }

                if (candidates.Count > 0)
                {
                    int index = randomNumbers.Next(candidates.Count);
                    string description = groupDescriptions.ElementAt(index);
                    int row = candidateRowIndices.ElementAt(index);
                    int col = candidateColIndices.ElementAt(index);
                    int digit = candidates.ElementAt(index);
                    int rowToWrite = row + row / 3 + 1;
                    int colToWrite = col + col / 3 + 1;

                    string message = $"{description} can contain {digit} only at ({row + 1}, {col + 1}).";

                    int stateIndex = 9 * row + col;
                    state[stateIndex] = digit;
                    candidateMasks[stateIndex] = 0;
                    board[rowToWrite][colToWrite] = (char) ('0' + digit);

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

                List<AppleSauce2> groups =
                    twoDigitMasks
                        .SelectMany(mask =>
                            cellGroups
                                .Where(group => @group.Count(tuple => candidateMasks[tuple.Index] == mask) == 2)
                                .Where(group => @group.Any(tuple =>
                                    candidateMasks[tuple.Index] != mask && (candidateMasks[tuple.Index] & mask) > 0))
                                .Select(group => new AppleSauce2(mask, @group.Key, @group.First().Description, @group)))
                        .ToList();

                if (groups.Any())
                {
                    foreach (AppleSauce2 group in groups)
                    {
                        List<AppleSauce1> cells =
                            @group.Cells
                                .Where(
                                    cell =>
                                        candidateMasks[cell.Index] != @group.Mask &&
                                        (candidateMasks[cell.Index] & @group.Mask) > 0)
                                .ToList();

                        AppleSauce1[] maskCells =
                            @group.Cells
                                .Where(cell => candidateMasks[cell.Index] == @group.Mask)
                                .ToArray();


                        if (cells.Any())
                        {
                            int upper = 0;
                            int lower = 0;
                            int temp = @group.Mask;

                            int value = 1;
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
                                $"Values {lower} and {upper} in {@group.Description} are in cells ({maskCells[0].Row + 1}, {maskCells[0].Column + 1}) and ({maskCells[1].Row + 1}, {maskCells[1].Column + 1}).");

                            foreach (AppleSauce1 cell in cells)
                            {
                                int maskToRemove = candidateMasks[cell.Index] & @group.Mask;
                                List<int> valuesToRemove = new List<int>();
                                int curValue = 1;
                                while (maskToRemove > 0)
                                {
                                    if ((maskToRemove & 1) > 0)
                                    {
                                        valuesToRemove.Add(curValue);
                                    }

                                    maskToRemove = maskToRemove >> 1;
                                    curValue += 1;
                                }

                                string valuesReport = string.Join(", ", valuesToRemove.ToArray());
                                Console.WriteLine(
                                    $"{valuesReport} cannot appear in ({cell.Row + 1}, {cell.Column + 1}).");

                                candidateMasks[cell.Index] &= ~@group.Mask;
                                stepChangeMade = true;
                            }
                        }
                    }
                }
            }

            #endregion

            return stepChangeMade;
        }

        private static int[] GenerateInitialBoardFromCompletelySolvedOne(Random randomNumbers,
            out int[] finalState, SudokuBoardAndStackState sudokuBoardAndState)
        {
            #region Generate inital board from the completely solved one

            // Board is solved at this point.
            // Now pick subset of digits as the starting position.
            int remainingDigits = 30;
            int maxRemovedPerBlock = 6;
            int[,] removedPerBlock = new int[3, 3];
            int[] positions = Enumerable.Range(0, 9 * 9).ToArray();
            int[] state = sudokuBoardAndState.StateStack.Peek();

            finalState = new int[state.Length];
            Array.Copy(state, finalState, finalState.Length);

            int removedPos = 0;
            while (removedPos < 9 * 9 - remainingDigits)
            {
                int curRemainingDigits = positions.Length - removedPos;
                int indexToPick = removedPos + randomNumbers.Next(curRemainingDigits);

                int row = positions[indexToPick] / 9;
                int col = positions[indexToPick] % 9;

                int blockRowToRemove = row / 3;
                int blockColToRemove = col / 3;

                if (removedPerBlock[blockRowToRemove, blockColToRemove] >= maxRemovedPerBlock)
                    continue;

                removedPerBlock[blockRowToRemove, blockColToRemove] += 1;

                int temp = positions[removedPos];
                positions[removedPos] = positions[indexToPick];
                positions[indexToPick] = temp;

                int rowToWrite = row + row / 3 + 1;
                int colToWrite = col + col / 3 + 1;

                sudokuBoardAndState.SudokuBoard.SetElementAt(rowToWrite,colToWrite, '.');

                int stateIndex = 9 * row + col;
                state[stateIndex] = 0;

                removedPos += 1;
            }

            Console.WriteLine();
            Console.WriteLine("Starting look of the board to solve:");
            Console.WriteLine(string.Join(Environment.NewLine, sudokuBoardAndState.SudokuBoard.ToString()));

            #endregion

            return state;
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

                List<AppleSauce3> groupsWithNMasks =
                    masks
                        .SelectMany(mask =>
                            cellGroups
                                .Where(group => @group.All(cell =>
                                    state[cell.Index] == 0 || (mask & (1 << (state[cell.Index] - 1))) == 0))
                                .Select(group => new AppleSauce3(mask, @group.First().Description, @group,
                                    @group.Where(cell =>
                                            state[cell.Index] == 0 && (candidateMasks[cell.Index] & mask) != 0)
                                        .ToList(), @group.Count(
                                        cell => state[cell.Index] == 0 &&
                                                (candidateMasks[cell.Index] & mask) != 0 &&
                                                (candidateMasks[cell.Index] & ~mask) != 0))))
                        .Where(group => @group.CellsWithMask.Count() == maskToOnesCount[@group.Mask])
                        .ToList();

                foreach (AppleSauce3 groupWithNMasks in groupsWithNMasks)
                {
                    int mask = groupWithNMasks.Mask;

                    if (groupWithNMasks.Cells
                        .Any(cell =>
                            (candidateMasks[cell.Index] & mask) != 0 &&
                            (candidateMasks[cell.Index] & ~mask) != 0))
                    {
                        StringBuilder message = new StringBuilder();
                        message.Append($"In {groupWithNMasks.Description} values ");

                        string separator = string.Empty;
                        int temp = mask;
                        int curValue = 1;
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
                        foreach (AppleSauce1 cell in groupWithNMasks.CellsWithMask)
                        {
                            message.Append($" ({cell.Row + 1}, {cell.Column + 1})");
                        }

                        message.Append(" and other values cannot appear in those cells.");

                        Console.WriteLine(message.ToString());
                    }

                    foreach (AppleSauce1 cell in groupWithNMasks.CellsWithMask)
                    {
                        int maskToClear = candidateMasks[cell.Index] & ~groupWithNMasks.Mask;
                        if (maskToClear == 0)
                            continue;

                        candidateMasks[cell.Index] &= groupWithNMasks.Mask;
                        stepChangeMade = true;

                        int valueToClear = 1;

                        string separator = string.Empty;
                        StringBuilder message = new StringBuilder();

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
            Dictionary<int, int> maskToOnesCount, int[] finalState, int[] state, char[][] board)
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

                Queue<int> candidateIndex1 = new Queue<int>();
                Queue<int> candidateIndex2 = new Queue<int>();
                Queue<int> candidateDigit1 = new Queue<int>();
                Queue<int> candidateDigit2 = new Queue<int>();

                for (int i = 0; i < candidateMasks.Length - 1; i++)
                {
                    if (maskToOnesCount[candidateMasks[i]] == 2)
                    {
                        int row = i / 9;
                        int col = i % 9;
                        int blockIndex = 3 * (row / 3) + col / 3;

                        int temp = candidateMasks[i];
                        int lower = 0;
                        int upper = 0;
                        for (int digit = 1; temp > 0; digit++)
                        {
                            if ((temp & 1) != 0)
                            {
                                lower = upper;
                                upper = digit;
                            }

                            temp = temp >> 1;
                        }

                        for (int j = i + 1; j < candidateMasks.Length; j++)
                        {
                            if (candidateMasks[j] == candidateMasks[i])
                            {
                                int row1 = j / 9;
                                int col1 = j % 9;
                                int blockIndex1 = 3 * (row1 / 3) + col1 / 3;

                                if (row == row1 || col == col1 || blockIndex == blockIndex1)
                                {
                                    candidateIndex1.Enqueue(i);
                                    candidateIndex2.Enqueue(j);
                                    candidateDigit1.Enqueue(lower);
                                    candidateDigit2.Enqueue(upper);
                                }
                            }
                        }
                    }
                }

                // At this point we have the lists with pairs of cells that might pick one of two digits each
                // Now we have to check whether that is really true - does the board have two solutions?

                List<int> stateIndex1 = new List<int>();
                List<int> stateIndex2 = new List<int>();
                List<int> value1 = new List<int>();
                List<int> value2 = new List<int>();

                while (candidateIndex1.Any())
                {
                    int index1 = candidateIndex1.Dequeue();
                    int index2 = candidateIndex2.Dequeue();
                    int digit1 = candidateDigit1.Dequeue();
                    int digit2 = candidateDigit2.Dequeue();

                    int[] alternateState = new int[finalState.Length];
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

                    var command = Command.ExpandCommandName;
                    var commandObj = Command.Expand;
                    while (command != Command.CompleteCommandName && command != Command.FailCommandName)
                    {
                        if (command == Command.ExpandCommandName)
                        {
                            int[] currentState = new int[9 * 9];

                            if (stateStack.Any())
                            {
                                Array.Copy(stateStack.Peek(), currentState, currentState.Length);
                            }
                            else
                            {
                                Array.Copy(alternateState, currentState, currentState.Length);
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

                                        int blockDigit =
                                            currentState[(blockRow * 3 + i / 3) * 9 + (blockCol * 3 + i % 3)];
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
                                stateStack.Push(currentState);
                                rowIndexStack.Push(bestRow);
                                colIndexStack.Push(bestCol);
                                usedDigitsStack.Push(bestUsedDigits);
                                lastDigitStack.Push(0); // No digit was tried at this position
                            }

                            // Always try to move after expand
                            command = Command.MoveCommandName;
                            commandObj = Command.Move;
                        }
                        else if (command == "collapse")
                        {
                            stateStack.Pop();
                            rowIndexStack.Pop();
                            colIndexStack.Pop();
                            usedDigitsStack.Pop();
                            lastDigitStack.Pop();

                            if (stateStack.Any())
                            {
                                command = Command.MoveCommandName; // Always try to move after collapse
                                commandObj = Command.Move;
                            }
                            else
                            {
                                command = Command.FailCommandName;
                                commandObj = Command.Fail;
                            }
                        }
                        else if (command == Command.MoveCommandName)
                        {
                            int rowToMove = rowIndexStack.Peek();
                            int colToMove = colIndexStack.Peek();
                            int digitToMove = lastDigitStack.Pop();

                            int rowToWrite = rowToMove + rowToMove / 3 + 1;
                            int colToWrite = colToMove + colToMove / 3 + 1;

                            bool[] usedDigits = usedDigitsStack.Peek();
                            int[] currentState = stateStack.Peek();
                            int currentStateIndex = 9 * rowToMove + colToMove;

                            int movedToDigit = digitToMove + 1;
                            while (movedToDigit <= 9 && usedDigits[movedToDigit - 1])
                                movedToDigit += 1;

                            if (digitToMove > 0)
                            {
                                usedDigits[digitToMove - 1] = false;
                                currentState[currentStateIndex] = 0;
                                board[rowToWrite][colToWrite] = '.';
                            }

                            if (movedToDigit <= 9)
                            {
                                lastDigitStack.Push(movedToDigit);
                                usedDigits[movedToDigit - 1] = true;
                                currentState[currentStateIndex] = movedToDigit;
                                board[rowToWrite][colToWrite] = (char) ('0' + movedToDigit);

                                if (currentState.Any(digit => digit == 0))
                                    command = Command.ExpandCommandName;
                                else
                                    command = Command.CompleteCommandName;
                            }
                            else
                            {
                                // No viable candidate was found at current position - pop it in the next iteration
                                lastDigitStack.Push(0);
                                command = "collapse";
                            }
                        }
                    }

                    if (command == Command.CompleteCommandName)
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
                    int pos = randomNumbers.Next(stateIndex1.Count());
                    int index1 = stateIndex1.ElementAt(pos);
                    int index2 = stateIndex2.ElementAt(pos);
                    int digit1 = value1.ElementAt(pos);
                    int digit2 = value2.ElementAt(pos);
                    int row1 = index1 / 9;
                    int col1 = index1 % 9;
                    int row2 = index2 / 9;
                    int col2 = index2 % 9;

                    string description = string.Empty;

                    if (index1 / 9 == index2 / 9)
                    {
                        description = $"row #{index1 / 9 + 1}";
                    }
                    else if (index1 % 9 == index2 % 9)
                    {
                        description = $"column #{index1 % 9 + 1}";
                    }
                    else
                    {
                        description = $"block ({row1 / 3 + 1}, {col1 / 3 + 1})";
                    }

                    state[index1] = finalState[index1];
                    state[index2] = finalState[index2];
                    candidateMasks[index1] = 0;
                    candidateMasks[index2] = 0;
                    changeMade = true;

                    for (int i = 0; i < state.Length; i++)
                    {
                        int tempRow = i / 9;
                        int tempCol = i % 9;
                        int rowToWrite = tempRow + tempRow / 3 + 1;
                        int colToWrite = tempCol + tempCol / 3 + 1;

                        board[rowToWrite][colToWrite] = '.';
                        if (state[i] > 0)
                            board[rowToWrite][colToWrite] = (char) ('0' + state[i]);
                    }

                    Console.WriteLine(
                        $"Guessing that {digit1} and {digit2} are arbitrary in {description} (multiple solutions): Pick {finalState[index1]}->({row1 + 1}, {col1 + 1}), {finalState[index2]}->({row2 + 1}, {col2 + 1}).");
                }
            }

            #endregion

            return changeMade;
        }

        private static void PrintBoardChange(bool changeMade, char[][] board)
        {
            if (changeMade)
            {
                #region Print the board as it looks after one change was made to it

                Console.WriteLine(string.Join(Environment.NewLine, board.Select(s => new string(s)).ToArray()));
                string code =
                    string.Join(string.Empty, board.Select(s => new string(s)).ToArray())
                        .Replace("-", string.Empty)
                        .Replace("+", string.Empty)
                        .Replace("|", string.Empty)
                        .Replace(".", "0");

                Console.WriteLine("Code: {0}", code);
                Console.WriteLine();

                #endregion
            }
        }

        static void Main(string[] args)
        {
            Play(new Random());

            Console.WriteLine();
            Console.Write("Press ENTER to exit... ");
            Console.ReadLine();
        }
    }
}