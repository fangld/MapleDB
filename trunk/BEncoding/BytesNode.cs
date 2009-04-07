using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BEncoding.Properties;

namespace BEncoding
{
    /// <summary>
    /// B编码的字节流类
    /// </summary>
    public class BytesNode : BEncodedNode, IComparable<BytesNode>, IEquatable<BytesNode>
    {
        #region Fields

        /// <summary>
        /// 字节数组
        /// </summary>
        private byte[] bytes;

        /// <summary>
        /// null字符类型编码
        /// </summary>
        private static readonly byte[] nullBytes;

        /// <summary>
        /// 是否为空
        /// </summary>
        private bool isNullable;

        /// <summary>
        /// 正则表达式
        /// </summary>
        private static readonly Regex regex;
        #endregion

        #region Exception strings

        private static readonly string ExBytesNodeLengthError =
            Resources.ResourceManager.GetString("BytesNodeLengthError");

        private static readonly string ExBytesNodeIntError =
            Resources.ResourceManager.GetString("BytesNodeIntError");

        #endregion

        #region Properties

        public static readonly BytesNode NullBytesNode;

        /// <summary>
        /// 字节数组的访问器
        /// </summary>
        public byte[] ByteArray
        {
            get { return bytes; }
            set
            {
                isNullable = (value == null);
                bytes = value;
            }
        }

        /// <summary>
        /// 字符串的访问器
        /// </summary>
        public string StringText
        {
            get
            {
                return isNullable ? null : BEncode.Encoding.GetString(bytes);
            }
            set
            {
                isNullable = (value == null);
                bytes = value != null ? BEncode.Encoding.GetBytes(value) : null;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        static BytesNode()
        {
            //字符为"-1:"
            nullBytes = new byte[] { 45, 49, 58 };
            NullBytesNode = new BytesNode();
            NullBytesNode.isNullable = false;

            regex = new Regex("^(-1|0|[1-9][0-9]*)$", RegexOptions.Compiled);
        }

        /// <summary>
        /// 构造函数,定义元素类型为字节流类型
        /// </summary>
        public BytesNode()
        {
            isNullable = true;
        }

        /// <summary>
        /// 构造函数,定义元素类型为字节流类型
        /// </summary>
        /// <param name="value">字节流</param>
        public BytesNode(byte[] value)
        {
            isNullable = (value == null);
            bytes = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">字符串</param>
        public BytesNode(string value)
            :
            this(BEncode.Encoding.GetBytes(value))
        { }

        #endregion

        #region Methods

        public static implicit operator BytesNode(string value)
        {
            return value != null ? new BytesNode(value) : NullBytesNode;
        }

        public static implicit operator BytesNode(byte[] value)
        {
            return value != null ? new BytesNode(value) : NullBytesNode;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 解码B编码字节流类型
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;
            StringBuilder sb = new StringBuilder();

            try
            {
                //当遇到字符':'(ASCII码为58),整数部分的解析结束
                do
                {
                    sb.Append((char)source[position]);
                    position++;
                } while (source[position] != 58);

                //跳过字符':'
                position++;
            }

                //当捕捉IndexOutOfRangeException,抛出BEncodingException
            catch (IndexOutOfRangeException)
            {
                throw new BEncodingException(ExBytesNodeLengthError);
            }

            //判断整数解析的正确性,错误则抛出异常
            if (!regex.IsMatch(sb.ToString()))
            {
                throw new BEncodingException(ExBytesNodeIntError);
            }

            //保存字符串长度
            int length = int.Parse(sb.ToString());

            if (length == -1)
            {
                ByteArray = null;
            }

            else
            {
                bytes = new byte[length];

                //开始解析字节数组
                try
                {
                    if (length > 0)
                    {
                        int index = position;
                        int byteArrayStart = position;
                        position += length;
                        while (index < position)
                        {
                            bytes[index - byteArrayStart] = source[index++];
                        }
                    }
                    isNullable = false;
                }

                //当捕捉IndexOutOfRangeException,抛出BEncodingException
                catch (IndexOutOfRangeException)
                {
                    throw new BEncodingException(ExBytesNodeLengthError);
                }
            }


            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler字符串类的编码函数
        /// </summary>
        /// <param name="stream">待编码的内存写入流</param>
        public override void Encode(Stream stream)
        {
            if (bytes != null)
            {
                byte[] op = BEncode.Encoding.GetBytes(string.Format("{0:d}:", bytes.Length));
                stream.Write(op, 0, op.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                stream.Write(nullBytes, 0, 3);
            }
        }

        #region IComparable<ByteArrayHandler> Members

        public int CompareTo(BytesNode other)
        {
            return StringText.CompareTo(other.StringText);
        }

        #endregion

        #region IEquatable<ByteArrayHandler> Members

        public bool Equals(BytesNode other)
        {
            if (other.StringText == null && StringText == null)
                return true;

            if (other.StringText == null || StringText == null)
                return false;

            return StringText.Equals(other.StringText);
        }

        #endregion

        public override int GetHashCode()
        {
            int result = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                result += bytes[i];
            }
            return result;
        }

        #endregion
    }
}
