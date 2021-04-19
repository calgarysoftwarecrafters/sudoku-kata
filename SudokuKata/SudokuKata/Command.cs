namespace SudokuKata
{
    public class Command
    {
        protected bool Equals(Command other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Command) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public string Name { get; }

        public Command(string name)
        {
            Name = name;
        }

        public static readonly Command Expand = new Command(ExpandCommandName);
        public static readonly Command Collapse = new Command(CollapseCommandName);
        public static readonly Command Move = new Command(MoveCommandName);
        public static readonly Command Complete = new Command(CompleteCommandName);
        public static readonly Command Fail = new Command(FailCommandName);

        public const string ExpandCommandName = "expand";
        public const string CollapseCommandName = "collapse";
        public const string MoveCommandName = "move";
        public const string CompleteCommandName = "complete";
        public const string FailCommandName = "fail";
    }
}