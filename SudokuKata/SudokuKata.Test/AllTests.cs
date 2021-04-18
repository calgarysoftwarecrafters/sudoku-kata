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
            var rng = new Random(990);
            Program.Play(rng);
            Approvals.Verify(output);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            Console.SetOut(_existingOut);
        }
    }
}