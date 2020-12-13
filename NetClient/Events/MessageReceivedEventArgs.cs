using System.Net.Sockets;

namespace NetClient.Events
{
    public class MessageReceivedEventArgs
    {
        public MessageReceivedEventArgs(int messageId, NetworkStream stream)
        {
            MessageId = messageId;
            Stream = stream;
        }

        public int MessageId { get; }

        public NetworkStream Stream { get; }
    }
}
