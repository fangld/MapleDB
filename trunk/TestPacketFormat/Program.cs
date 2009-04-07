using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Packets;

namespace TestPacketFormat
{
    class Program
    {
        static void Main(string[] args)
        {
            File.Delete("c:\\formmater.txt");
            IPEndPoint endPoint = new IPEndPoint(12, 8500);
            byte[] packet = new byte[256];
            for (int i= 0; i < 256; i++)
            {
                packet[i] = (byte)i;
            }
            Console.WriteLine("Finish");
            FileStream fs = new FileStream("c:\\formmater.txt", FileMode.OpenOrCreate);
            byte[] writeBytes = PacketFormmater.Format(packet, endPoint);
            fs.Write(writeBytes, 0, writeBytes.Length);
            fs.Write(writeBytes, 0, writeBytes.Length);
        }
    }
}
