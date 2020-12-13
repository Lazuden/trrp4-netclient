using System;
using System.Net.Sockets;

namespace NetClient.Events
{
    public class ConnectionFailedEventArgs
    {
        public ConnectionFailedEventArgs(Exception cause)
        {
            Cause = cause;
        }

        public Exception Cause { get; }
    }
}
