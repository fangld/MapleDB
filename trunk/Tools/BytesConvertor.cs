using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    /// <summary>
    /// 字节流转换器
    /// </summary>
    public static class BytesConvertor
    {
        /// <summary>
        /// 将32位有符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的32位有符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="startIndex">写入字节流的位置</param>
        public static void Int32ToBytes(int value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[++startIndex] = (byte)((value >> 16) & 0xFF);
            buffer[++startIndex] = (byte)((value >> 8) & 0xFFFF);
            buffer[++startIndex] = (byte)(value & 0xFFFFFF);
        }

        /// <summary>
        /// 将字节流转换为32位有符号整数
        /// </summary>
        /// <param name="buffer">待读入的字节流</param>
        /// <param name="startOffset">待读入字节流的位置</param>
        /// <returns>返回32位有符号整数</returns>
        public static int BytesToInt32(byte[] buffer, int startOffset)
        {
            int result = 0x0;
            result |= buffer[startOffset] << 24;
            result |= buffer[++startOffset] << 16;
            result |= buffer[++startOffset] << 8;
            result |= buffer[++startOffset];
            return result;
        }

        /// <summary>
        /// 将32位无符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的32位无符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="startIndex">写入字节流的位置</param>
        public static void UInt32ToBytes(uint value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[++startIndex] = (byte)((value >> 16) & 0xFF);
            buffer[++startIndex] = (byte)((value >> 8) & 0xFFFF);
            buffer[++startIndex] = (byte)(value & 0xFFFFFF);
        }

        /// <summary>
        /// 将字节流转换为32位无符号整数
        /// </summary>
        /// <param name="buffer">待读入的字节流</param>
        /// <param name="startOffset">待读入字节流的位置</param>
        /// <returns>返回32位无符号整数</returns>
        public static uint BytesToUInt32(byte[] buffer, int startOffset)
        {
            uint result = 0x0;
            result |= (uint)buffer[startOffset] << 24;
            result |= (uint)buffer[++startOffset] << 16;
            result |= (uint)buffer[++startOffset] << 8;
            result |= buffer[++startOffset];
            return result;
        }
    }
}
