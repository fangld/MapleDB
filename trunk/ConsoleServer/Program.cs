using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DbProxy.Server;

namespace ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TextWriter writer = new StreamWriter("C:\\server.log");
            Console.SetOut(writer);
            DbProxyServer server =
                new DbProxyServer("Data Source=localhost;Initial Catalog=TestDbProxy;Persist Security Info=True;User ID=sa;Password=gznt", 1000);
            server.Listen();
            writer.Close();
            writer.Dispose();
        }
    }
}
