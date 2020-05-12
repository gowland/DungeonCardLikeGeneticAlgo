namespace Game
{
    public class DirectionResult
    {
        public DirectionResult(Direction direction)
        {
            Direction = direction;
            Success = true;
        }

        public DirectionResult()
        {
        }

        public Direction? Direction { get; }
        public bool Success { get; }

        public static readonly DirectionResult Failed = new DirectionResult();
    }
}