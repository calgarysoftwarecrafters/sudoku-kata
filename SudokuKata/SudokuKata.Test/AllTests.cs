using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuKata;
using ApprovalTests;

namespace SudokuKata.Test
{
    [TestClass]
    public class AllSudokuTests
    {
        private TextWriter _existingOut;

        [TestInitialize()]
        public void Initialize()
        {
            _existingOut = Console.Out;
        }

        [TestMethod]
        public void pinEverythingTests()
        {
            StringWriter output = new StringWriter();
            Console.SetOut(output);
            for (int i = 900; i < 1200; i++)
            {
                var rng = new Random(i);
                Program.Play(rng);    
            }
            Approvals.Verify(output);
        }

        [TestMethod]
        public void testEmptyBoard()
        {
            Approvals.Verify(new SudokuBoardAndStackState());
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            Console.SetOut(_existingOut);
        }
    }
}