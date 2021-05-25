using System.Collections.Generic;

namespace SudokuKata
{
    public class AppleSauce1
    {
        public AppleSauce1(int discriminator, string description, int index, int row, int column)
        {
            Discriminator = discriminator;
            Description = description;
            Index = index;
            Row = row;
            Column = column;
        }

        public int Discriminator { get; }
        public string Description { get; }
        public int Index { get; }
        public int Row { get; }
        public int Column { get; }

        public override string ToString()
        {
            return
                $"{{ Discriminator = {Discriminator}, Description = {Description}, Index = {Index}, Row = {Row}, Column = {Column} }}";
        }

        public override bool Equals(object value)
        {
            var type = value as AppleSauce1;
            return type != null && EqualityComparer<int>.Default.Equals(type.Discriminator, Discriminator) &&
                   EqualityComparer<string>.Default.Equals(type.Description, Description) &&
                   EqualityComparer<int>.Default.Equals(type.Index, Index) &&
                   EqualityComparer<int>.Default.Equals(type.Row, Row) &&
                   EqualityComparer<int>.Default.Equals(type.Column, Column);
        }

        public override int GetHashCode()
        {
            var num = 0x7a2f0b42;
            num = -1521134295 * num + EqualityComparer<int>.Default.GetHashCode(Discriminator);
            num = -1521134295 * num + EqualityComparer<string>.Default.GetHashCode(Description);
            num = -1521134295 * num + EqualityComparer<int>.Default.GetHashCode(Index);
            num = -1521134295 * num + EqualityComparer<int>.Default.GetHashCode(Row);
            return -1521134295 * num + EqualityComparer<int>.Default.GetHashCode(Column);
        }
    }
}