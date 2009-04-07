using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using TestDbProxy;

namespace TestDbProxyServer
{
    class Program
    {
        public static byte[] ABytes;

        public static byte[] BBytes;

        static void Main(string[] args)
        {
            byte start = byte.Parse(args[0]);
            byte end = byte.Parse(args[1]);
            ABytes = new byte[50];
            BBytes = new byte[50];
            for (int i = 0; i < 50; i++)
            {
                ABytes[i] = (byte) 'A';
                BBytes[i] = (byte) 'B';
            }

            for (byte i = start; i <= end; i++)
            {
                Thread thread = new Thread(TestTranscode);
                thread.Start(i.ToString());
            }
        }

        static void TestTranscode(object id)
        {
            string pk = id.ToString();
            DbProxyDal.DeleteProc(pk);
            int i = 0;
            while (true)
            {
                if (i % 50 == 0)
                {
                    Thread.Sleep(20000);
                }
                DbProxyDal.InsertProcNull(pk);
                DbProxyDal.UpdateProcNormal(pk, long.MaxValue, int.MaxValue, short.MaxValue, byte.MaxValue, DateTime.Now,
                                            0.0f, 0.0, pk, "0123456789", ABytes, BBytes);
                DbProxyDal.DeleteProc(pk);
                i++;
                Console.WriteLine("Thread id:{0}, time:{1}", pk, i);
            }
        }
    }
}
