using System;
using System.Collections.Generic;
using System.Text;

namespace TestBEncoding
{
    public static class ExceptionStrings
    {
        #region Bytes strings

        public const string BytesNodeIntError = "B编码字节流的整数部分解析错误!";
        public const string BytesNodeLengthError = "B编码字节流的字节数组长度异常!";

        #endregion

        #region Int strings

        public const string IntNodeLengthError = "B编码整数类的字节数组长度异常!";
        public const string IntNodeMatchError = "B编码整数类的匹配错误!";

        #endregion

        #region List strings

        public const string ListNodeLengthError = "B编码列表类的字节数组长度异常!";

        #endregion

        #region Dict strings

        public const string DictNodeKeyLengthError = "待添加的字符串关系字长度为0!";
        public const string DictNodeLengthError = "B编码字典类的字节数组长度异常!";
        public const string DictNodeSameKey = "B编码字典类中包含相同的字符串关键字!";
        public const string DictNodeValueIsNull = "待添加的B编码节点为空!";

        #endregion

        public const string BytesFirstByteError = "待解码的字节数组的首位字节异常!";
        public const string BytesLengthZero = "待解码的字节数值长度为零!";
        public const string BytesLengthTooLong = "待解码的字节数值长度过长!";
    }
}
