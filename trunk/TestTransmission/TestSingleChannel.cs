using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Transmission;

namespace TestTransmission
{
    [TestFixture]
    public class TestSingleChannel
    {
        private SingleChannelServer server;

        private const int port = 9100;

        [SetUp]
        public void SetUp()
        {
            server = new SingleChannelServer(port);
            server.Start(1);
            server.BeginAccept(DoAcceptClientCallBack, server);
        }

        [TearDown]
        public void TearDown()
        {
            server.Stop();
        }

        public void DoAcceptClientCallBack(IAsyncResult ar)
        {
            SingleChannelServer server = (SingleChannelServer)ar.AsyncState;
            SingleChannelClient client = server.EndAccept(ar);
            byte[] bytes1 = Encoding.Default.GetBytes("This is channel");
            client.Send(bytes1);
        }

        [Test]
        public void TestReceive()
        {
            SingleChannelClient client = new SingleChannelClient();
            client.Connect("localhost", port);
            byte[] bytes = client.Receive();
            string msg = Encoding.Default.GetString(bytes);
            Assert.AreEqual("This is channel", msg);
            client.Close();
        }
    }
}
