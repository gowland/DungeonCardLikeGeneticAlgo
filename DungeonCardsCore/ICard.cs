namespace Game
{
    public interface ICard<out TType>
    {
        TType Type { get; }
        int Value { get; set; }
    }
}