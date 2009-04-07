using System;
using System.Collections.Generic;
using System.Threading;
using Timer = System.Timers.Timer;

namespace DbProxy.Client
{
    /// <summary>
    /// DB Proxy连接池
    /// </summary>
    public static class TdsConnectionPool
    {
        private const double clearTimeout = 6000000;

        private static Timer timer;

        private static ReaderWriterLock contentLock;

        private static ReaderWriterLock listLock;

        private static Dictionary<int, LinkedList<ITdsConnection>> contents;

        static TdsConnectionPool()
        {
            contentLock = new ReaderWriterLock();
            listLock = new ReaderWriterLock();
            contents = new Dictionary<int, LinkedList<ITdsConnection>>();
            timer = new Timer(clearTimeout);
            timer.Enabled = true;
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Clear();
        }

        private static int GetKey(string url, ushort port, string deptId, byte priority, int timeout)
        {
            int result = 0;

            result += priority*13;

            result += port*37;

            result += timeout*73;

            for (int i = 0; i < url.Length; i++)
            {
                result += url[i]*103;
            }

            for (int i = 0; i < deptId.Length; i++)
            {
                result += deptId[i]*137;
            }

            return result;
        }

        internal static ITdsConnection GetConnection(string url, ushort port, string deptId, byte priority, int timeout)
        {
            timer.Stop();
            LinkedList<ITdsConnection> connectionList;
            int key = GetKey(url, port, deptId, priority, timeout);

            contentLock.AcquireReaderLock(-1);
            if (contents.ContainsKey(key))
            {
                connectionList = contents[key];
            }
            else
            {
                connectionList = new LinkedList<ITdsConnection>();
                contents.Add(key, connectionList);
            }
            contentLock.ReleaseReaderLock();

            ITdsConnection connection;

            listLock.AcquireReaderLock(-1);
            if (connectionList.Count > 0)
            {
                connection = connectionList.First.Value;
                connectionList.RemoveFirst();
            }
            else
            {
                connection = new TdsConnection(url, port, deptId, priority, timeout);
            }
            listLock.ReleaseReaderLock();

            return new PooledTdsConnection(connection);
        }

        internal static void PutConnection(ITdsConnection connection)
        {
            timer.Start();
            LinkedList<ITdsConnection> connectionList;
            int key = connection.GetHashCode();

            contentLock.AcquireWriterLock(-1);
            connectionList = contents[key];
            contentLock.ReleaseWriterLock();

            listLock.AcquireWriterLock(-1);
            connectionList.AddFirst(connection);
            listLock.ReleaseWriterLock();
        }

        public static void Clear()
        {
            Console.WriteLine("Pool clear start!");
            timer.Stop();
            contentLock.AcquireWriterLock(-1);
            foreach (LinkedList<ITdsConnection> list in contents.Values)
            {
                while (list.Count != 0)
                {
                    LinkedListNode<ITdsConnection> node = list.First;
                    ITdsConnection connection = node.Value;
                    list.RemoveFirst();
                    connection.Close();
                }
            }
            contentLock.ReleaseWriterLock();
            timer.Start();
            Console.WriteLine("Pool clear end!");
        }

        public static void Close()
        {
            Clear();
            timer.Stop();
        }
    }
}
