using System;
using System.Collections.Generic;
using System.Text;
using BEncoding;
using Tools;

namespace Transmission
{
    /// <summary>
    /// �������ݰ�������
    /// </summary>
    public static class HandshakeHandler
    {
        /// <summary>
        /// �½��������ݰ�
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="iv">��ʼ������</param>
        /// <returns>�����������ݰ�</returns>
        public static byte[] BuildHandshake(byte[] key, byte[] iv)
        {
            DictNode node = new DictNode();
            node.Add("key", key);
            node.Add("iv", iv);
            return BEncode.ByteArrayEncode(node);
        }

        /// <summary>
        /// ��֤�������ݰ�
        /// </summary>
        /// <param name="handshake">�������ݰ�</param>
        /// <param name="key">��Կ</param>
        /// <param name="iv">��ʼ������</param>
        /// <returns>�����֤�ɹ�����true;���򷵻�false</returns>
        public static bool VerifyHandshake(byte[] handshake, byte[] key, byte[] iv)
        {
            DictNode node = (DictNode) BEncode.Decode(handshake);
            byte[] handshakeKey = ((BytesNode) node["key"]).ByteArray;
            byte[] handshakeIV = ((BytesNode) node["iv"]).ByteArray;
            return BytesComparer.Compare(handshakeKey, key) && BytesComparer.Compare(handshakeIV, iv);
        }
    }
}
