using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Transmission;

namespace TestTransmission
{
    [TestFixture]
    public class TestDoubleChannel
    {
        private DoubleChannelServer server;

        private const int port = 9100;

        [SetUp]
        public void SetUp()
        {
            server = new DoubleChannelServer(port);
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
            DoubleChannelServer server = (DoubleChannelServer)ar.AsyncState;
            DoubleChannelClient client = server.EndAccept(ar);
            byte[] bytes1 = Encoding.Default.GetBytes("This is channel1");
            byte[] bytes2 = Encoding.Default.GetBytes("This is channel2");
            client.SendByChannel1(bytes1);
            client.SendByChannel2(bytes2);
        }

        [Test]
        public void TestReceive()
        {
            DoubleChannelClient client = new DoubleChannelClient();
            client.Connect("localhost", port);
            byte[] bytes1 = client.ReceiveByChannel1();
            byte[] bytes2 = client.ReceiveByChannel2();
            string msg1 = Encoding.Default.GetString(bytes1);
            string msg2 = Encoding.Default.GetString(bytes2);
            Assert.AreEqual("This is channel1", msg1);
            Assert.AreEqual("This is channel2", msg2);
            client.Close();
        }
    }
}
