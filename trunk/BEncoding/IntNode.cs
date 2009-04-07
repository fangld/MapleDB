using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BEncoding.Properties;

namespace BEncoding
{
    /// <summary>
    /// Handler整数类
    /// </summary>
    public class IntNode : BEncodedNode
    {
        #region Fields

        /// <summary>
        /// 整数
        /// </summary>
        private int value;

        /// <summary>
        /// 正则表达式
        /// </summary>
        private static readonly Regex regex;

        #endregion

        #region Exception strings

        private static readonly string ExIntNodeLengthError;

        private static readonly string ExIntNodeMatchError;

        #endregion

        #region Properties

        /// <summary>
        /// 整数访问器
        /// </summary>
        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

        #endregion

        #region Constructors

        static IntNode()
        {
            ExIntNodeLengthError = Resources.ResourceManager.GetString("IntNodeLengthError");
            ExIntNodeMatchError = Resources.ResourceManager.GetString("IntNodeMatchError");
            regex = new Regex("^(0|-?[1-9][0-9]*)$", RegexOptions.Compiled);
        }

        /// <summary>
        /// 构造函数,定义元素类型为整数类型
        /// </summary>
        public IntNode() { }

        /// <summary>
        /// 构造函数,定义元素类型为整数类型
        /// </summary>
        /// <param name="value">整数</param>
        public IntNode(int value)
        {
            this.value = value;
        }

        #endregion

        #region Methods

        public static implicit operator IntNode(int value)
        {
            return new IntNode(value);
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Handler整数类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;
            StringBuilder sb = new StringBuilder();

            //跳过字符'i'
            position++;

            try
            {
                //当遇到字符'e'(ASCII码为101),解析结束
                do
                {
                    sb.Append((char)source[position]);
                    position++;
                } while (source[position] != 101);

                //跳过字符'e'
                position++;
            }

            //当捕捉IndexOutOfRangeException,抛出BEncodingException
            catch (IndexOutOfRangeException)
            {
                throw new BEncodingException(ExIntNodeLengthError);
            }

            //判断整数解析的正确性,错误则抛出异常
            if (regex.IsMatch(sb.ToString()))
            {
                //保存整数
                value = int.Parse(sb.ToString());

                //返回所解析的数组长度
                return position - start;
            }

            throw new BEncodingException(ExIntNodeMatchError);
        }

        /// <summary>
        /// Handler整数类的编码函数
        /// </summary>
        /// <param name="stream">待编码的内存写入流</param>
        public override void Encode(Stream stream)
        {
            byte[] op = Encoding.Default.GetBytes(string.Format("i{0:d}e", value));
            stream.Write(op, 0, op.Length);
        }

        #endregion
    }
}
