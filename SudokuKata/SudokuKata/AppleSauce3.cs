using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    public class AppleSauce3
    {
        public int Mask { get; }
        public string Description { get; }
        public IGrouping<int, AppleSauce1> Cells { get; }
        public List<AppleSauce1> CellsWithMask { get; }
        public int CleanableCellsCount { get; }

        public AppleSauce3(int mask, string description, IGrouping<int, AppleSauce1> cells, List<AppleSauce1> cellsWithMask, int cleanableCellsCount)
        {
            Mask = mask;
            Description = description;
            Cells = cells;
            CellsWithMask = cellsWithMask;
            CleanableCellsCount = cleanableCellsCount;
        }

        public override string ToString()
        {
            return $"{{ Mask = {Mask}, Description = {Description}, Cells = {Cells}, CellsWithMask = {CellsWithMask}, CleanableCellsCount = {CleanableCellsCount} }}";
        }

        public override bool Equals(object value)
        {
            var type = value as AppleSauce3;
            return (type != null) && EqualityComparer<int>.Default.Equals(type.Mask, Mask) && EqualityComparer<string>.Default.Equals(type.Description, Description) && EqualityComparer<IGrouping<int, AppleSauce1>>.Default.Equals(type.Cells, Cells) && EqualityComparer<List<AppleSauce1>>.Default.Equals(type.CellsWithMask, CellsWithMask) && EqualityComparer<int>.Default.Equals(type.CleanableCellsCount, CleanableCellsCount);
        }

        public override int GetHashCode()
        {
            int num = 0x7a2f0b42;
            num = (-1521134295 * num) + EqualityComparer<int>.Default.GetHashCode(Mask);
            num = (-1521134295 * num) + EqualityComparer<string>.Default.GetHashCode(Description);
            num = (-1521134295 * num) + EqualityComparer<IGrouping<int, AppleSauce1>>.Default.GetHashCode(Cells);
            num = (-1521134295 * num) + EqualityComparer<List<AppleSauce1>>.Default.GetHashCode(CellsWithMask);
            return (-1521134295 * num) + EqualityComparer<int>.Default.GetHashCode(CleanableCellsCount);
        }
    }
}