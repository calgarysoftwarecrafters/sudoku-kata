using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    public class AppleSauce2
    {
        public int Mask { get; }
        public int Discriminator { get; }
        public string Description { get; }
        public IGrouping<int, AppleSauce1> Cells { get; }

        public AppleSauce2(int mask, int discriminator, string description, IGrouping<int, AppleSauce1> cells)
        {
            Mask = mask;
            Discriminator = discriminator;
            Description = description;
            Cells = cells;
        }

        public override string ToString()
        {
            return $"{{ Mask = {Mask}, Discriminator = {Discriminator}, Description = {Description}, Cells = {Cells} }}";
        }

        public override bool Equals(object value)
        {
            var type = value as AppleSauce2;
            return (type != null) && EqualityComparer<int>.Default.Equals(type.Mask, Mask) && EqualityComparer<int>.Default.Equals(type.Discriminator, Discriminator) && EqualityComparer<string>.Default.Equals(type.Description, Description) && EqualityComparer<IGrouping<int, AppleSauce1>>.Default.Equals(type.Cells, Cells);
        }

        public override int GetHashCode()
        {
            int num = 0x7a2f0b42;
            num = (-1521134295 * num) + EqualityComparer<int>.Default.GetHashCode(Mask);
            num = (-1521134295 * num) + EqualityComparer<int>.Default.GetHashCode(Discriminator);
            num = (-1521134295 * num) + EqualityComparer<string>.Default.GetHashCode(Description);
            return (-1521134295 * num) + EqualityComparer<IGrouping<int, AppleSauce1>>.Default.GetHashCode(Cells);
        }
    }
}