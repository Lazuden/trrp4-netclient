using System.IO;

namespace NetClient.Events
{
    public class ConnectionRefusedEventArgs
    {
        public ConnectionRefusedEventArgs(IOException cause)
        {
            Cause = cause;
        }

        public IOException Cause;
    }
}
