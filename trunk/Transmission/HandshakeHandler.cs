using System;
using System.Collections.Generic;
using System.Text;
using BEncoding;
using Tools;

namespace Transmission
{
    /// <summary>
    /// 握手数据包处理器
    /// </summary>
    public static class HandshakeHandler
    {
        /// <summary>
        /// 新建握手数据包
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>返回握手数据包</returns>
        public static byte[] BuildHandshake(byte[] key, byte[] iv)
        {
            DictNode node = new DictNode();
            node.Add("key", key);
            node.Add("iv", iv);
            return BEncode.ByteArrayEncode(node);
        }

        /// <summary>
        /// 验证握手数据包
        /// </summary>
        /// <param name="handshake">握手数据包</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>如果验证成功返回true;否则返回false</returns>
        public static bool VerifyHandshake(byte[] handshake, byte[] key, byte[] iv)
        {
            DictNode node = (DictNode) BEncode.Decode(handshake);
            byte[] handshakeKey = ((BytesNode) node["key"]).ByteArray;
            byte[] handshakeIV = ((BytesNode) node["iv"]).ByteArray;
            return BytesComparer.Compare(handshakeKey, key) && BytesComparer.Compare(handshakeIV, iv);
        }
    }
}
