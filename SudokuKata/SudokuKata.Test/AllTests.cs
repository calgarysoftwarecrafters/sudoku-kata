using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SudokuKata.Test
{
    [TestClass]
    public class AllSudokuTests
    {
        private TextWriter _existingOut;

        [TestInitialize]
        public void Initialize()
        {
            _existingOut = Console.Out;
        }

        [TestMethod]
        public void PinEverythingTests()
        {
            var output = new StringWriter();
            Console.SetOut(output);
            for (var i = 900; i < 1200; i++)
            {
                var rng = new Random(i);
                Program.Play(rng);
            }

            Approvals.Verify(output);
        }

        [TestMethod]
        public void ConstructFullyPopulatedBoardTests()
        {
            var output = new StringWriter();
            Console.SetOut(output);
            for (var i = 1300; i < 1600; i++)
            {
                var rng = new Random(i);
                var sudokuBoardAndStackState = new SudokuBoardAndStackState();
                sudokuBoardAndStackState.ConstructFullySolvedBoard(rng);
                output.WriteLine(sudokuBoardAndStackState.SudokuBoard.ToString());
                output.WriteLine(StateStackString(sudokuBoardAndStackState.StateStack));
            }

            Approvals.Verify(output);
        }

        [TestMethod]
        public void TestEmptyBoard()
        {
            Approvals.Verify(new SudokuBoard());
        }

        [TestMethod]
        public void TestToString()
        {
            var sudokuBoard = new SudokuBoard();
            sudokuBoard.SetValueAt(1, 1, 2);
            sudokuBoard.SetValueAt(4, 5, 3);
            Approvals.Verify(sudokuBoard);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Console.SetOut(_existingOut);
        }

        private string StateStackString(Stack<int[]> stateStack)
        {
            return string.Join(Environment.NewLine, stateStack.Select(SingleStackElementString).ToArray());
        }

        private string SingleStackElementString(int[] stackElement)
        {
            return string.Join(",", stackElement.Select(value => value.ToString()).ToArray());
        }
    }
}