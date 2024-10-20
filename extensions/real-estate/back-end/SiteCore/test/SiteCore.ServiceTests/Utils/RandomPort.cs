using System.Net;
using System.Net.Sockets;

namespace SiteCore.ServiceTests.Utils
{
    public class RandomPort
    {
        public static int GetRandomUnUsedPort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }
    }
}

