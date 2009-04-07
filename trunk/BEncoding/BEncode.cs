using System;
using System.IO;
using System.Text;
using BEncoding.Properties;

namespace BEncoding
{
    /// <summary>
    /// Handler的生成工厂
    /// </summary>
    public static class BEncode
    {
        /// <summary>
        /// 编码类型
        /// </summary>
        internal static readonly Encoding Encoding = Encoding.GetEncoding("GB2312");

        #region Exception strings

        private static readonly string ExBytesFirstByteError =
            Resources.ResourceManager.GetString("BytesFirstByteError");

        private static readonly string ExBytesLengthZero =
            Resources.ResourceManager.GetString("BytesLengthZero");

        private static readonly string ExBytesLengthTooLong =
            Resources.ResourceManager.GetString("BytesLengthTooLong");

        #endregion

        /// <summary>
        /// 解码函数
        /// </summary>
        /// <param name="source">待解码的字符串</param>
        /// <returns>返回已解码的Handler基类</returns>
        public static BEncodedNode Decode(string source)
        {
            return Decode(Encoding.GetBytes(source));
        }

        /// <summary>
        /// 解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <returns>返回已解码的节点</returns>
        public static BEncodedNode Decode(byte[] source)
        {
            //初始化变量
            int position = 0;

            ///当待解码字节数组的长度为零,抛出异常
            if (source.Length == 0)
                throw new BEncodingException(ExBytesLengthZero);

            BEncodedNode result = Decode(source, ref position);

            if (position != source.Length)
                throw new BEncodingException(ExBytesLengthTooLong);

            return result;
        }

        /// <summary>
        /// 解码函数
        /// </summary>
        /// <param name="stream">待解码的流</param>
        /// <returns>返回已解码的节点</returns>
        public static BEncodedNode Decode(Stream stream)
        {
            StreamReader sr = new StreamReader(stream, Encoding);
            string str = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            return Decode(str);
        }

        /// <summary>
        /// 解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的位置</param>
        /// <returns>返回Handler基类类型</returns>
        internal static BEncodedNode Decode(byte[] source, ref int position)
        {
            byte b = source[position];

            BEncodedNode result;

            //如果source[position] == '{'(ASCII码为123),就返回ListNode
            if (b == 123)
            {
                result = new ListNode();
            }

            //如果source[position] == '['(ASCII码为91,就返回DictionaryNode
            else if (b == 91)
            {
                result = new DictNode();
            }

            //如果source[position] == 'i'(ASCII码为105),就返回IntNode
            else if (b == 105)
            {
                result = new IntNode();
            }

            //如果source[position] == '-'(ASCII码为45),就返回NullBytesNode
            else if (b == 45)
            {
                result = BytesNode.NullBytesNode;
            }

            //如果source[position] == '0' - '9'(ASCII码为48 - 57),就返回BytesNode
            else if (b >= 48 && b <= 57)
            {
                result = new BytesNode();
            }

            //其它的情况,抛出异常
            else
            {
                throw new BEncodingException(ExBytesFirstByteError);
            }

            result.Decode(source, ref position);
            return result;
        }

        /// <summary>
        /// 编码函数,返回字节流
        /// </summary>
        /// <param name="source">待编码的B编码节点</param>
        /// <returns>已编码的字节流</returns>
        public static byte[] ByteArrayEncode(BEncodedNode source)
        {
            MemoryStream result = new MemoryStream();
            source.Encode(result);
            return result.ToArray();
        }

        /// <summary>
        /// 编码函数,返回字符串
        /// </summary>
        /// <param name="source">待编码的Handler对象</param>
        /// <returns>已编码的字符串</returns>
        public static string StringEncode(BEncodedNode source)
        {
            return Encoding.GetString(ByteArrayEncode(source));
        }
    }
}