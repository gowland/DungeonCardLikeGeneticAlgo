using Game;

namespace DungeonCardsGeneticAlgo.Support
{
    public interface IGameAgent
    {
        DirectionResult GetDirectionFromAlgo(Board board);
    }
}