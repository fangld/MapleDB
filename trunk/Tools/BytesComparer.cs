using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public static class BytesComparer
    {
        public static bool Compare(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
                return false;
            for (int i = 0; i <  bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
