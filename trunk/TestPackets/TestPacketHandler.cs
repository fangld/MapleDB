using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Packets;

namespace TestPackets
{
    [TestFixture]
    public class TestPacketHandler
    {
        // Thread signal.
        public static ManualResetEvent clientConnected = new ManualResetEvent(false);

        private TcpListener listener;

        private const int port = 12345;

        [SetUp]
        public void SetUp()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start(64);
            listener.BeginAcceptSocket(DoAcceptSocketCallback, listener);
        }

        [TearDown]
        public void TearDown()
        {
            listener.Stop();
        }

        // Process the client connection.
        public static void DoAcceptSocketCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            // End the operation and display the received data on the
            //console.
            Socket client = listener.EndAcceptSocket(ar);

            PacketHandler serverHandler = new PacketHandler(client);

            byte[] helloBytes = Encoding.Default.GetBytes("Hello world!");
            serverHandler.SendNormalPacket(helloBytes);
            serverHandler.SendNormalPacket(serverHandler.IV);
            serverHandler.SendNormalPacket(serverHandler.Key);
            serverHandler.SendSealedPacket(helloBytes);

            // Process the connection here. (Add the client to a 
            // server table, read data, etc.)
            Console.WriteLine("Client connected completed");

            // Signal the calling thread to continue.
            clientConnected.Set();
        }

        [Test]
        public void TestPacket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("localhost", port);

            PacketHandler handler = new PacketHandler(socket);
            byte[] normalPacket = handler.ReceiveNormalPacket();
            string normalMsg = Encoding.Default.GetString(normalPacket);
            Assert.AreEqual("Hello world!", normalMsg);
            byte[] iv = handler.ReceiveNormalPacket();
            byte[] key = handler.ReceiveNormalPacket();
            handler.SetKeyIV(key, iv);
            byte[] sealedPacket = handler.ReceiveSealedPacket();
            string sealedMsg = Encoding.Default.GetString(sealedPacket);
            Assert.AreEqual("Hello world!", sealedMsg);
        }
    }
}