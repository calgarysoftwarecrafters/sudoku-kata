using System;
using System.IO;
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
                var sudokuBoardAndStackState = new SudokuBoardSolver();
                sudokuBoardAndStackState.ConstructFullySolvedBoard(rng);
                output.WriteLine(sudokuBoardAndStackState.ToString());
                output.WriteLine(sudokuBoardAndStackState.CurrentBoardState());
            }

            Approvals.Verify(output);
        }

        [TestMethod]
        public void BoardGetStateTests()
        {
            var sudokuBoard = new SudokuBoard();
            sudokuBoard.SetValueAt(0, 1, 3);
            sudokuBoard.SetValueAt(2, 2, 2);
            sudokuBoard.SetValueAt(8, 8, 1);
            sudokuBoard.SetValueAt(0, 0, 5);
            
            var boardState = sudokuBoard.GetState();
            
            Approvals.Verify(string.Join("",boardState));
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
    }
}