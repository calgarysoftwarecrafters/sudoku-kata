using System.Collections.Generic;

namespace SudokuKata
{
    public class LookupStructures
    {
        public LookupStructures(Dictionary<int, int> singleBitToIndex, int allOnes,
            Dictionary<int, int> maskToOnesCount)
        {
            SingleBitToIndex = singleBitToIndex;
            AllOnes = allOnes;
            MaskToOnesCount = maskToOnesCount;
        }

        public Dictionary<int, int> SingleBitToIndex { get; }
        public int AllOnes { get; }
        public Dictionary<int, int> MaskToOnesCount { get; }

        public static LookupStructures PrepareLookupStructures()
        {
            #region Prepare lookup structures that will be used in further execution

            var maskToOnesCount = new Dictionary<int, int>();
            maskToOnesCount[0] = 0;
            for (var i = 1; i < 1 << 9; i++)
            {
                var smaller = i >> 1;
                var increment = i & 1;
                maskToOnesCount[i] = maskToOnesCount[smaller] + increment;
            }

            var singleBitToIndex = new Dictionary<int, int>();
            for (var i = 0; i < 9; i++)
                singleBitToIndex[1 << i] = i;

            var allOnes = (1 << 9) - 1;

            #endregion

            return new LookupStructures(singleBitToIndex, allOnes, maskToOnesCount);
        }
    }
}