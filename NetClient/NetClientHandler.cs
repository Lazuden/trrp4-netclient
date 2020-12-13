using NetClient.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetClient
{
    public sealed class NetClientHandler
    {
        private TcpClient _tcpClient;
        private Thread _thread;
        private bool _manualRefusal;

        public NetClientHandler()
        {
        }

        public delegate void ConnectionFailedEventHandler(object sender, ConnectionFailedEventArgs e);
        public delegate void ConnectionSucceededEventHandler(object sender, ConnectionSucceededEventArgs e);
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public delegate void ConnectionClosedEventHandler(object sender, ConnectionClosedEventArgs e);
        public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);
        public delegate void ConnectionRefusedEventHandler(object sender, ConnectionRefusedEventArgs e);

        public event ConnectionFailedEventHandler ConnectionFailed;
        public event ConnectionSucceededEventHandler ConnectionSucceeded;
        public event MessageReceivedEventHandler MessageReceived;
        public event ConnectionClosedEventHandler ConnectionClosed;
        public event DisconnectedEventHandler Disconnected;
        public event ConnectionRefusedEventHandler ConnectionRefused;

        public void Connect(IPEndPoint ipEndPoint)
        {
            _tcpClient = new TcpClient();
            _tcpClient.ReceiveBufferSize = 256 * 256 * 32; /* omg */
            try
            {
                _tcpClient.Connect(ipEndPoint);
                ConnectionSucceeded?.Invoke(this, new ConnectionSucceededEventArgs());
            }
            catch (Exception e)
            {
                ConnectionFailed?.Invoke(this, new ConnectionFailedEventArgs(e));
                _tcpClient = null;
            }

            if (_tcpClient != null)
            {
                _manualRefusal = false;
                _thread = new Thread(Run);
                _thread.Start();
            }
        }

        public void Disconnect()
        {
            if (_tcpClient != null)
            {
                _manualRefusal = true;
                _tcpClient.Close();
                //_thread.Join();
                _tcpClient = null;
                _thread = null;
            }
        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    NetworkStream stream = _tcpClient.GetStream();
                    int messageId = stream.ReadByte();
                    Console.WriteLine(messageId);
                    if (messageId == -1)
                    {
                        ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs());
                        break;
                    }
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageId, stream));
                }
            }
            catch (IOException e)
            {
                if (_manualRefusal)
                {
                    Disconnected?.Invoke(this, new DisconnectedEventArgs());
                }
                else
                {
                    ConnectionRefused?.Invoke(this, new ConnectionRefusedEventArgs(e));
                }
            }
            catch
            {
                ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs());
            }
        }

        public void SendMessage(IMessage message)
        {
            if (_tcpClient != null)
            {
                try
                {
                    byte[] bytes = message.Serialize();
                    NetworkStream stream = _tcpClient.GetStream();
                    stream.WriteByte(message.Id);
                    stream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception) { /* ingored */ }
            }
        }
    }

    public static class NetworkStreamExtension
    {
        public static byte[] ReadNBytes(this NetworkStream stream, int size)
        {
            byte[] bytes = new byte[size];
            int readTotal = 0;
            while (true)
            {
                int left = size - readTotal;
                if (left <= 0)
                {
                    break;
                }
                readTotal += stream.Read(bytes, readTotal, left);
            }
            return bytes;
        }
    }
}
