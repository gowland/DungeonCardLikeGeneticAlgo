namespace Game
{
    public class Card<TType> : ICard<TType>
    {
        public Card(TType type, int value)
        {
            Type = type;
            Value = value;
        }
        public TType Type { get; }
        public int Value { get; set; }

        public Card<TType> AdjustValue(int valueAdjustment)
        {
            return new Card<TType>(Type, Value + valueAdjustment);
        }
    }

    public interface ISlot<TCard>
    {
        TCard Card { get; set; }
    }

    public class Slot<TCard> : ISlot<TCard> where TCard: class
    {
        public TCard Card { get; set; }
    }

    public enum CardType
    {
        Monster,
        Weapon,
        Player,
        Gold
    }
}
