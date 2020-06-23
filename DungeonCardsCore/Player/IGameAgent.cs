namespace Game.Player
{
    public interface IGameAgent
    {
        DirectionResult GetDirectionFromAlgo(Board board);
    }
}