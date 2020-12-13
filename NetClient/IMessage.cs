namespace NetClient
{
    public interface IMessage
    {
        byte Id { get; }

        byte[] Serialize();
    }
}
