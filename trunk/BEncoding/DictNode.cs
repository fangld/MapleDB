using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BEncoding.Properties;

namespace BEncoding
{
    /// <summary>
    /// B编码字典类
    /// </summary>
    public class DictNode : BEncodedNode, IDictionary<BytesNode, BEncodedNode>
    {
        #region Fields

        /// <summary>
        /// string, Node字典
        /// </summary>
        private readonly IDictionary<BytesNode, BEncodedNode> dict;

        #endregion

        #region Exception strings

        private static readonly string ExDictNodeKeyLengthError =
            Resources.ResourceManager.GetString("DictNodeKeyLengthError");

        private static readonly string ExDictNodeValueIsNull =
            Resources.ResourceManager.GetString("DictNodeValueIsNull");

        private static readonly string ExDictNodeLengthError =
            Resources.ResourceManager.GetString("DictNodeLengthError");

        private static readonly string ExDictNodeSameKey =
            Resources.ResourceManager.GetString("DictNodeSameKey");

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DictNode()
        {
            dict = new Dictionary<BytesNode, BEncodedNode>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dictNode">ByteArray, BEncodedNode字典</param>
        public DictNode(IDictionary<BytesNode, BEncodedNode> dictNode)
        {
            dict = dictNode;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key">Handler关键字</param>
        /// <param name="value">Handler节点</param>
        public DictNode(BytesNode key, BEncodedNode value)
            : this()
        {
            dict.Add(key, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 添加B编码节点函数,并且关键字为字符串
        /// </summary>
        /// <param name="key">待添加的字符串关键字</param>
        /// <param name="value">待添加的B编码节点</param>
        public void Add(BytesNode key, BEncodedNode value)
        {
            dict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return ContainsKey(new BytesNode(key));
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Handler字典类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;

            //跳过字符'['
            position++;

            try
            {
                //当遇到']'(ASCII码为93),解析结束
                while (source[position] != 93)
                {
                    //解析字符串
                    BytesNode keyNode = new BytesNode();
                    keyNode.Decode(source, ref position);
                    byte[] key = keyNode.ByteArray;
                    if (key.LongLength == 0)
                    {
                        throw new BEncodingException(ExDictNodeKeyLengthError);
                    }

                    //解析值节点
                    BEncodedNode valueNode = BEncode.Decode(source, ref position);

                    //']'(ASCII码为93),解析结束
                    if (valueNode == null)
                    {
                        throw new BEncodingException(ExDictNodeValueIsNull);
                    }

                    //字典添加key和handler
                    dict.Add(keyNode, valueNode);
                }
            }

            //当捕捉IndexOutOfRangeException,抛出BEncodingException
            catch (IndexOutOfRangeException)
            {
                throw new BEncodingException(ExDictNodeLengthError);
            }

            //当捕捉ArgumentException,抛出BEncodingException
            catch (ArgumentException)
            {
                throw new BEncodingException(ExDictNodeSameKey);
            }

            //跳过字符'e'
            position++;

            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler字典类的编码函数
        /// </summary>
        /// <param name="stream">待编码的内存写入流</param>
        public override void Encode(Stream stream)
        {
            //向内存流写入'['(ASCII码为91)
            stream.WriteByte(91);

            //对于每一个Handler进行编码
            foreach (BytesNode key in dict.Keys)
            {
                key.Encode(stream);
                dict[key].Encode(stream);
            }

            //向内存流写入']'(ASCII码为93)
            stream.WriteByte(93);
        }

        #endregion

        #region IDictionary<BytesNode, BEncodedNode> Members

        public bool ContainsKey(BytesNode key)
        {
            return dict.ContainsKey(key);
        }

        public ICollection<BytesNode> Keys
        {
            get { return dict.Keys; }
        }

        public bool Remove(BytesNode key)
        {
            return dict.Remove(key);
        }

        public bool TryGetValue(BytesNode key, out BEncodedNode value)
        {
            return dict.TryGetValue(key, out value);
        }

        public ICollection<BEncodedNode> Values
        {
            get { return dict.Values; }
        }

        public BEncodedNode this[BytesNode key]
        {
            get
            {
                return dict[key];
            }
            set
            {
                dict[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<BytesNode, BEncodedNode>> Members

        public void Add(KeyValuePair<BytesNode, BEncodedNode> item)
        {
            dict.Add(item);
        }

        public void Clear()
        {
            dict.Clear();
        }

        public bool Contains(KeyValuePair<BytesNode, BEncodedNode> item)
        {
            return Contains(item);
        }

        public void CopyTo(KeyValuePair<BytesNode, BEncodedNode>[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return dict.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<BytesNode, BEncodedNode> item)
        {
            return Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<BytesNode, BEncodedNode>> Members

        public IEnumerator<KeyValuePair<BytesNode, BEncodedNode>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        #endregion
    }
}