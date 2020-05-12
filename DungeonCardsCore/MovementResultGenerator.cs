namespace Game
{
    public interface IMovementResultGenerator<TCard>
    {
        MovementResult GetResult(TCard card);
    }

    public class MovementResultGenerator : IMovementResultGenerator<ICard<CardType>>
    {
        public MovementResult GetResult(ICard<CardType> card)
        {
            if (card == null)
            {
                return MovementResult.Attack; // TODO: Remove 
            }
            if (card.Type == CardType.Gold || card.Type == CardType.Weapon)
            {
                return MovementResult.Collect;
            }
            
            return MovementResult.Attack;
        }
    }

    public enum MovementResult
    {
        Attack,
        Collect
    }
}