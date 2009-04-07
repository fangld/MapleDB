using System;
using System.Net.Sockets;
using System.Threading;
using DbProxy.Client;
using DbProxy.Server;
using NUnit.Framework;

namespace TestDbProxy
{
    [TestFixture]
    public class TestDbProxy
    {
        private Semaphore stopped = new Semaphore(1, 1);

        long bigIntValue;
        int intValue;
        short smallIntValue;
        byte tinyIntValue;
        DateTime dateTimeValue;
        float realValue;
        double floatValue;
        string varcharText;
        string charText;
        byte[] varbinaryStream;
        byte[] binaryStream;

        bool bigIntNull;
        bool intNull;
        bool smallIntNull;
        bool tinyIntNull;
        bool dateTimeNull;
        bool realNull;
        bool floatNull;
        bool varcharNull;
        bool charNull;
        bool varbinaryNull;
        bool binaryNull;

        private DbProxyServer server;

        private const string dbConnString = "Data Source=localhost;Initial Catalog=TestDbProxy;Persist Security Info=True;User ID=sa;Password=gznt;Timeout=10";

        private const int timeout = 1000;

        private static readonly byte[] BBytes;

        private static readonly byte[] ABytes;

        static TestDbProxy()
        {
            ABytes = new byte[50];
            BBytes = new byte[50];
            for (int i = 0; i < 50; i++)
            {
                ABytes[i] = (byte) 'A';
                BBytes[i] = (byte) 'B';
            }
        }

        [SetUp]
        public void SetUp()
        {
            stopped.WaitOne();
            server = new DbProxyServer(dbConnString, timeout);
            server.Start();
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("Tear down start!");
            TdsConnectionPool.Clear();
            server.Stop();
            stopped.Release();
            Console.WriteLine("Tear down end!");
        }

        [Test]
        public void TestTextInsertAndDeleteNormal()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertTextNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                             0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            DbProxyDal.QueryTextNormalByReader("001", out bigIntValue, out intValue, out smallIntValue, out tinyIntValue, out dateTimeValue,
                              out realValue, out floatValue, out varcharText, out charText, out varbinaryStream,
                              out binaryStream);
            Assert.AreEqual(long.MaxValue, bigIntValue);
            Assert.AreEqual(int.MaxValue, intValue);
            Assert.AreEqual(short.MaxValue, smallIntValue);
            Assert.AreEqual(byte.MaxValue, tinyIntValue);
            Assert.AreEqual(testDateTime, dateTimeValue);
            Assert.AreEqual(0.0f, realValue);
            Assert.AreEqual(0.0, floatValue);
            Assert.AreEqual("0123456789", varcharText);
            Assert.AreEqual("0123456789", charText);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual((byte)'A', varbinaryStream[i]);
                Assert.AreEqual((byte)'A', binaryStream[i]);
            }
            DbProxyDal.DeleteText("001");
        }

        [Test]
        public void TestTextUpdateNormal()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertTextNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                             0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            DbProxyDal.UpdateTextNormal("001", long.MinValue, int.MinValue, short.MinValue, byte.MinValue, testDateTime,
                             1.0f, 1.0, "9876543210", "9876543210", BBytes, BBytes);
            DbProxyDal.QueryTextNormalByReader("001", out bigIntValue, out intValue, out smallIntValue, out tinyIntValue, out dateTimeValue,
                              out realValue, out floatValue, out varcharText, out charText, out varbinaryStream,
                              out binaryStream);
            Assert.AreEqual(long.MinValue, bigIntValue);
            Assert.AreEqual(int.MinValue, intValue);
            Assert.AreEqual(short.MinValue, smallIntValue);
            Assert.AreEqual(byte.MinValue, tinyIntValue);
            Assert.AreEqual(testDateTime, dateTimeValue);
            Assert.AreEqual(1.0f, realValue);
            Assert.AreEqual(1.0, floatValue);
            Assert.AreEqual("9876543210", varcharText);
            Assert.AreEqual("9876543210", charText);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual((byte)'B', varbinaryStream[i]);
                Assert.AreEqual((byte)'B', binaryStream[i]);
            }
            DbProxyDal.DeleteText("001");
        }

        [Test]
        public void TestTextInsertAndDeleteNull()
        {
            DbProxyDal.InsertTextNull("001");
            DbProxyDal.QueryTextNullByReader("001", out bigIntNull, out intNull, out smallIntNull, out tinyIntNull, out dateTimeNull,
                              out realNull, out floatNull, out varcharNull, out charNull, out varbinaryNull,
                              out binaryNull);
            Assert.AreEqual(true, bigIntNull);
            Assert.AreEqual(true, intNull);
            Assert.AreEqual(true, smallIntNull);
            Assert.AreEqual(true, tinyIntNull);
            Assert.AreEqual(true, dateTimeNull);
            Assert.AreEqual(true, realNull);
            Assert.AreEqual(true, floatNull);
            Assert.AreEqual(true, varcharNull);
            Assert.AreEqual(true, charNull);
            Assert.AreEqual(true, varbinaryNull);
            Assert.AreEqual(true, binaryNull);
            DbProxyDal.DeleteText("001");
        }

        [Test]
        public void TestTextUpdateValueNull()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertTextNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                             0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            DbProxyDal.UpdateTextNull("001");
            DbProxyDal.QueryTextNullByReader("001", out bigIntNull, out intNull, out smallIntNull, out tinyIntNull, out dateTimeNull,
                              out realNull, out floatNull, out varcharNull, out charNull, out varbinaryNull,
                              out binaryNull);
            Assert.AreEqual(true, bigIntNull);
            Assert.AreEqual(true, intNull);
            Assert.AreEqual(true, smallIntNull);
            Assert.AreEqual(true, tinyIntNull);
            Assert.AreEqual(true, dateTimeNull);
            Assert.AreEqual(true, realNull);
            Assert.AreEqual(true, floatNull);
            Assert.AreEqual(true, varcharNull);
            Assert.AreEqual(true, charNull);
            Assert.AreEqual(true, varbinaryNull);
            Assert.AreEqual(true, binaryNull);
            DbProxyDal.DeleteText("001");
        }

        [Test]
        public void TestProcInsertAndDeleteNormal()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertProcNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                                        0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            DbProxyDal.QueryProcNormalByReader("001", out bigIntValue, out intValue, out smallIntValue, out tinyIntValue, out dateTimeValue,
                              out realValue, out floatValue, out varcharText, out charText, out varbinaryStream,
                              out binaryStream);
            Assert.AreEqual(long.MaxValue, bigIntValue);
            Assert.AreEqual(int.MaxValue, intValue);
            Assert.AreEqual(short.MaxValue, smallIntValue);
            Assert.AreEqual(byte.MaxValue, tinyIntValue);
            Assert.AreEqual(testDateTime, dateTimeValue);
            Assert.AreEqual(0.0f, realValue);
            Assert.AreEqual(0.0, floatValue);
            Assert.AreEqual("0123456789", varcharText);
            Assert.AreEqual("0123456789", charText);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual((byte)'A', varbinaryStream[i]);
                Assert.AreEqual((byte)'A', binaryStream[i]);
            }
            DbProxyDal.DeleteProc("001");
        }

        [Test]
        public void TestProcUpdateNormal()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertProcNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                             0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            DbProxyDal.UpdateProcNormal("001", long.MinValue, int.MinValue, short.MinValue, byte.MinValue, testDateTime,
                             1.0f, 1.0, "9876543210", "9876543210", BBytes, BBytes);

            DbProxyDal.QueryProcNormalByReader("001", out bigIntValue, out intValue, out smallIntValue, out tinyIntValue, out dateTimeValue,
                              out realValue, out floatValue, out varcharText, out charText, out varbinaryStream,
                              out binaryStream);
            Assert.AreEqual(long.MinValue, bigIntValue);
            Assert.AreEqual(int.MinValue, intValue);
            Assert.AreEqual(short.MinValue, smallIntValue);
            Assert.AreEqual(byte.MinValue, tinyIntValue);
            Assert.AreEqual(testDateTime, dateTimeValue);
            Assert.AreEqual(1.0f, realValue);
            Assert.AreEqual(1.0, floatValue);
            Assert.AreEqual("9876543210", varcharText);
            Assert.AreEqual("9876543210", charText);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual((byte)'B', varbinaryStream[i]);
                Assert.AreEqual((byte)'B', binaryStream[i]);
            }
            DbProxyDal.DeleteProc("001");
        }

        [Test]
        public void TestProcInsertAndDeleteNull()
        {
            DbProxyDal.InsertProcNull("001");
            DbProxyDal.QueryProcNullByReader("001", out bigIntNull, out intNull, out smallIntNull, out tinyIntNull, out dateTimeNull,
                              out realNull, out floatNull, out varcharNull, out charNull, out varbinaryNull,
                              out binaryNull);
            Assert.AreEqual(true, bigIntNull);
            Assert.AreEqual(true, intNull);
            Assert.AreEqual(true, smallIntNull);
            Assert.AreEqual(true, tinyIntNull);
            Assert.AreEqual(true, dateTimeNull);
            Assert.AreEqual(true, realNull);
            Assert.AreEqual(true, floatNull);
            Assert.AreEqual(true, varcharNull);
            Assert.AreEqual(true, charNull);
            Assert.AreEqual(true, varbinaryNull);
            Assert.AreEqual(true, binaryNull);
            DbProxyDal.DeleteProc("001");
        }

        [Test]
        public void TestProcUpdateValueNull()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertProcNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                             0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            DbProxyDal.UpdateProcNull("001");
            DbProxyDal.QueryProcNullByReader("001", out bigIntNull, out intNull, out smallIntNull, out tinyIntNull, out dateTimeNull,
                              out realNull, out floatNull, out varcharNull, out charNull, out varbinaryNull,
                              out binaryNull);
            Assert.AreEqual(true, bigIntNull);
            Assert.AreEqual(true, intNull);
            Assert.AreEqual(true, smallIntNull);
            Assert.AreEqual(true, tinyIntNull);
            Assert.AreEqual(true, dateTimeNull);
            Assert.AreEqual(true, realNull);
            Assert.AreEqual(true, floatNull);
            Assert.AreEqual(true, varcharNull);
            Assert.AreEqual(true, charNull);
            Assert.AreEqual(true, varbinaryNull);
            Assert.AreEqual(true, binaryNull);
            DbProxyDal.DeleteProc("001");
        }

        [Test]
        public void TestProcInsertAndDeleteOutput()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertProcNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                                        0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            DbProxyDal.QueryProcNormalByOutput("001", out bigIntValue, out intValue, out smallIntValue, out tinyIntValue,
                                               out dateTimeValue, out realValue, out floatValue, out varcharText,
                                               out charText, out varbinaryStream, out binaryStream);
            Assert.AreEqual(long.MaxValue, bigIntValue);
            Assert.AreEqual(int.MaxValue, intValue);
            Assert.AreEqual(short.MaxValue, smallIntValue);
            Assert.AreEqual(byte.MaxValue, tinyIntValue);
            Assert.AreEqual(testDateTime, dateTimeValue);
            Assert.AreEqual(0.0f, realValue);
            Assert.AreEqual(0.0, floatValue);
            Assert.AreEqual("0123456789", varcharText);
            Assert.AreEqual("0123456789", charText);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual((byte)'A', varbinaryStream[i]);
                Assert.AreEqual((byte)'A', binaryStream[i]);
            }
            DbProxyDal.DeleteProc("001");
        }

        [Test]
        public void TestProcInsertAndDeleteScalar()
        {
            DateTime testDateTime = new DateTime(2008, 8, 8, 8, 0, 0);
            DbProxyDal.InsertProcNormal("001", long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, testDateTime,
                                        0.0f, 0.0, "0123456789", "0123456789", ABytes, ABytes);
            int linenum, returnValue;
            DbProxyDal.QueryProcNormalByScalar("001", out linenum, out returnValue);
            Assert.AreEqual(1, linenum);
            Assert.AreEqual(1, returnValue);
            DbProxyDal.DeleteProc("001");
        }

        [Test]
        [ExpectedException(typeof(DbProxyException), ExpectedMessage = ExceptionStrings.AccessTimeout)]
        public void TestAccessTimeout()
        {
            DbProxyDal.AccessTimeout();
        }
    }
}
