using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using BEncoding;

namespace TestBEncoding
{
    [TestFixture]
    public class TestBEncode
    {
        #region 解码测试

        #region Handler解码测试
        //[Test]
        //public void TestDecodeHandler1()
        //{
        //    byte[] source = File.ReadAllBytes("test_dummy.zip.torrent");
        //    DictNode dh = (DictNode)BEncode.Decode(source);
        //    Assert.AreEqual("http://tracker.bittorrent.com:6969/announce", (dh["announce"] as BytesNode).StringText);
        //    Assert.AreEqual("http://tracker.bittorrent.com:6969/announce", Encoding.Default.GetString((dh["announce"] as BytesNode).ByteArray));
        //}

        /// <summary>
        /// Handler解码测试函数2,测试用例为""
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesLengthZero)]
        public void TestDecodeHandler2()
        {
            BEncode.Decode("");
        }

        /// <summary>
        /// Handler解码测试函数3,测试用例为"35208734823ljdahflajhdf"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesNodeLengthError)]
        public void TestDecodeHandler3()
        {
            BEncode.Decode("35208734823ljdahflajhdf");
        }
        #endregion

        #region 整数解码测试
        /// <summary>
        /// 整数解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeInteger1()
        {
            //Test1正整数
            IntNode ih1 = (IntNode)BEncode.Decode("i10e");
            Assert.AreEqual(ih1.Value, 10);

            //Test2零
            IntNode ih2 = (IntNode)BEncode.Decode("i0e");
            Assert.AreEqual(ih2.Value, 0);

            //Test3负整数
            IntNode ih3 = (IntNode)BEncode.Decode("i-55e");
            Assert.AreEqual(ih3.Value, -55);

            //Test4所有的数字
            IntNode ih4 = (IntNode)BEncode.Decode("i1234567890e");
            Assert.AreEqual(ih4.Value, 1234567890);
        }

        /// <summary>
        /// 整数解码测试函数2,测试用例为"ie"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.IntNodeLengthError)]
        public void TestDecodeInteger2()
        {
            BEncode.Decode("ie");
        }

        /// <summary>
        /// 整数解码测试函数3,测试用例为"i341foo382e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.IntNodeMatchError)]
        public void TestDecodeInteger3()
        {
            BEncode.Decode("i341foo382e");
        }

        /// <summary>
        /// 整数解码测试函数4,测试用例为"index-0e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.IntNodeMatchError)]
        public void TestDecodeInteger4()
        {
            BEncode.Decode("i-0e");
        }

        /// <summary>
        /// 整数解码测试函数5,测试用例为"i123"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.IntNodeLengthError)]
        public void TestDecodeInteger5()
        {
            BEncode.Decode("i123");
        }

        /// <summary>
        /// 整数解码测试函数6,测试用例为"i0345e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.IntNodeMatchError)]
        public void TestDecodeInteger6()
        {
            BEncode.Decode("i0345e");
        }
        #endregion

        #region 字节数组解码测试
        /// <summary>
        /// 字节数组解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeByteArray1()
        {
            //Test1
            BytesNode bah1 = (BytesNode)BEncode.Decode("10:0123456789");
            Assert.AreEqual(bah1.ByteArray, Encoding.Default.GetBytes("0123456789"));
            Assert.AreEqual(bah1.StringText, "0123456789");

            //Test2
            BytesNode bah2 = (BytesNode)BEncode.Decode("26:abcdefghijklmnopqrstuvwxyz");
            Assert.AreEqual(bah2.ByteArray, Encoding.Default.GetBytes("abcdefghijklmnopqrstuvwxyz"));
            Assert.AreEqual(bah2.StringText, "abcdefghijklmnopqrstuvwxyz");

            //Test3
            BytesNode bah3 = (BytesNode)BEncode.Decode("124:ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
            Assert.AreEqual(Encoding.Default.GetBytes("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９"), bah3.ByteArray);
            Assert.AreEqual("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９", bah3.StringText);

            //Test4
            BytesNode bah4 = (BytesNode)BEncode.Decode("0:");
            Assert.AreEqual(bah4.ByteArray, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual(bah4.StringText, string.Empty);
        }

        /// <summary>
        /// 字节数组解码测试函数2,测试用例为"2:abcedefg"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesLengthTooLong)]
        public void TestDecodeByteArray2()
        {
            BEncode.Decode("2:abcedefg");
        }

        /// <summary>
        /// 字节数组解码测试函数3,测试用例为"02:ab"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesNodeIntError)]
        public void TestDecodeByteArray3()
        {
            BEncode.Decode("02:ab");
        }

        /// <summary>
        /// 字节数组解码测试函数4,测试用例为"0:0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesLengthTooLong)]
        public void TestDecodeByteArray4()
        {
            BEncode.Decode("0:0:");
        }

        /// <summary>
        /// 字节数组解码测试函数5,测试用例为"9:abc"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesNodeLengthError)]
        public void TestDecodeByteArray5()
        {
            BEncode.Decode("9:abc");
        }
        #endregion

        #region 列表解码测试
        /// <summary>
        /// 列表解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeList1()
        {
            //Test1整数
            ListNode lh1 = (ListNode)BEncode.Decode("{i0ei1ei2e}");
            Assert.AreEqual(((IntNode)lh1[0]).Value, 0);
            Assert.AreEqual(((IntNode)lh1[1]).Value, 1);
            Assert.AreEqual(((IntNode)lh1[2]).Value, 2);

            //Test2字节数组
            ListNode lh2 = (ListNode)BEncode.Decode("{3:abc2:xy}");
            Assert.AreEqual((lh2[0] as BytesNode).ByteArray, Encoding.Default.GetBytes("abc"));
            Assert.AreEqual((lh2[0] as BytesNode).StringText, "abc");

            Assert.AreEqual((lh2[1] as BytesNode).ByteArray, Encoding.Default.GetBytes("xy"));
            Assert.AreEqual((lh2[1] as BytesNode).StringText, "xy");

            //Test3空字节数组
            ListNode lh3 = (ListNode)BEncode.Decode("{0:0:0:}");
            Assert.AreEqual((lh3[0] as BytesNode).ByteArray, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual((lh3[0] as BytesNode).StringText, string.Empty);

            Assert.AreEqual((lh3[1] as BytesNode).ByteArray, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual((lh3[1] as BytesNode).StringText, string.Empty);

            Assert.AreEqual((lh3[2] as BytesNode).ByteArray, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual((lh3[2] as BytesNode).StringText, string.Empty);

            //Test4字节数组与整数
            ListNode lh4 = (ListNode)BEncode.Decode("{{5:Alice3:Bob}{i2ei3e}}");
            ListNode lHandler40 = (ListNode)lh4[0];
            ListNode lHandler41 = (ListNode)lh4[1];

            Assert.AreEqual((lHandler40[0] as BytesNode).ByteArray, Encoding.Default.GetBytes("Alice"));
            Assert.AreEqual((lHandler40[0] as BytesNode).StringText, "Alice");

            Assert.AreEqual((lHandler40[1] as BytesNode).ByteArray, Encoding.Default.GetBytes("Bob"));
            Assert.AreEqual((lHandler40[1] as BytesNode).StringText, "Bob");

            Assert.AreEqual(((IntNode)lHandler41[0]).Value, 2);

            Assert.AreEqual(((IntNode)lHandler41[1]).Value, 3);

            //Test5空列表
            ListNode lh5 = (ListNode)BEncode.Decode("{}");
            Assert.AreEqual(lh5.Count, 0);
        }

        /// <summary>
        /// 列表解码测试函数2,测试用例为"{}zeral"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesLengthTooLong)]
        public void TestDecodeList2()
        {
            BEncode.Decode("{}zeral");
        }

        /// <summary>
        /// 列表解码测试函数3,测试用例为"{"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.ListNodeLengthError)]
        public void TestDecodeList3()
        {
            BEncode.Decode("{");
        }

        /// <summary>
        /// 列表解码测试函数4,测试用例为"{0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.ListNodeLengthError)]
        public void TestDecodeList4()
        {
            BEncode.Decode("{0:");
        }

        /// <summary>
        /// 列表解码测试函数5,测试用例为"{01:x}"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesNodeIntError)]
        public void TestDecodeList5()
        {
            BEncode.Decode("{01:x}");
        }
        #endregion

        #region 字典解码测试
        /// <summary>
        /// 字典解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeDictionary1()
        {
            //Test1整数
            DictNode dh1 = (DictNode)BEncode.Decode("[3:agei25e]");
            Assert.AreEqual(((IntNode)dh1["age"]).Value, 25);

            //Test2字节数组
            DictNode dh2 = (DictNode)BEncode.Decode("[3:agei25e5:color4:blue]");
            Assert.AreEqual(((IntNode)dh2["age"]).Value, 25);

            Assert.AreEqual((dh2["color"] as BytesNode).ByteArray, Encoding.Default.GetBytes("blue"));
            Assert.AreEqual((dh2["color"] as BytesNode).StringText, "blue");

            //Test3字节数组与整数
            DictNode dh3 = (DictNode)BEncode.Decode("[8:spam.mp3[6:author5:Alice6:lengthi1048576e]]");
            DictNode dHandler31 = (DictNode)dh3["spam.mp3"];
            Assert.AreEqual((dHandler31["author"] as BytesNode).ByteArray, Encoding.Default.GetBytes("Alice"));
            Assert.AreEqual((dHandler31["author"] as BytesNode).StringText, "Alice");
            Assert.AreEqual(((IntNode)dHandler31["length"]).Value, 1048576);

            //Test4空字典
            DictNode dh4 = (DictNode)BEncode.Decode("[]");
            Assert.AreEqual(dh4.Count, 0);
        }

        /// <summary>
        /// 字典解码测试函数2,测试用例为"[3:agei25e3:agei50e]"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.DictNodeSameKey)]
        public void TestDecodeDictionary2()
        {
            BEncode.Decode("[3:agei25e3:agei50e]");
        }

        /// <summary>
        /// 字典解码测试函数3,测试用例为"["
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.DictNodeLengthError)]
        public void TestDecodeDictionary3()
        {
            BEncode.Decode("[");
        }

        /// <summary>
        /// 字典解码测试函数4,测试用例为"[]0564adf"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesLengthTooLong)]
        public void TestDecodeDictionary4()
        {
            BEncode.Decode("[]0564adf");
        }

        /// <summary>
        /// 字典解码测试函数5,测试用例为"[3:foo]"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesFirstByteError)]
        public void TestDecodeDictionary5()
        {
            BEncode.Decode("[3:foo]");
        }

        /// <summary>
        /// 字典解码测试函数6,测试用例为"[i1e0:]"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesNodeIntError)]
        public void TestDecodeDictionary6()
        {
            BEncode.Decode("[i1e0:]");
        }

        /// <summary>
        /// 字典解码测试函数7,测试用例为"[0:1:a]"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.DictNodeKeyLengthError)]
        public void TestDecodeDictionary7()
        {
            BEncode.Decode("[0:1:a]");
        }

        /// <summary>
        /// 字典解码测试函数8,测试用例为"[0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.DictNodeKeyLengthError)]
        public void TestDecodeDictionary8()
        {
            BEncode.Decode("[0:");
        }

        /// <summary>
        /// 字典解码测试函数9,测试用例为"[01:x0:]"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesNodeIntError)]
        public void TestDecodeDictionary9()
        {
            BEncode.Decode("[01:x0:]");
        }

        /// <summary>
        /// 字典解码测试函数10,测试用例为"[i1ei0e]"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BEncodingException), ExpectedMessage = ExceptionStrings.BytesNodeLengthError)]
        public void TestDecodeDictionary10()
        {
            BEncode.Decode("[i1ei0e]");
        }

        #endregion

        #endregion

        #region 编码测试

        //[Test]
        //public void TestEncodeHandler1()
        //{
        //    FileStream sourceFile = File.OpenRead(@"test_dummy.zip.torrent");
        //    byte[] source = new byte[sourceFile.Length];
        //    sourceFile.Read(source, 0, (int)sourceFile.Length);
        //    sourceFile.Close();
        //    DictNode dh = (DictNode)BEncode.Decode(source);
        //    byte[] destion = BEncode.ByteArrayEncode(dh);
        //    FileStream targetFile = File.OpenWrite("i:\\test.torrent");
        //    targetFile.Write(destion, 0, destion.Length);

        //    int i;
        //    for (i = 0; i < source.Length; i++)
        //    {
        //        Assert.AreEqual(source[i], destion[i]);
        //    }

        //    targetFile.Close();
        //}

        /// <summary>
        /// 测试整数编码
        /// </summary>
        [Test]
        public void TestEncodeInteger1()
        {
            //Test1测试用例为4
            IntNode ih1 = new IntNode(4);
            string source1 = BEncode.StringEncode(ih1);
            Assert.AreEqual(source1, "i4e");

            //Test2测试用例为1234567890
            IntNode ih2 = new IntNode(1234567890);
            string source2 = BEncode.StringEncode(ih2);
            Assert.AreEqual(source2, "i1234567890e");

            //Test3测试用例为0
            IntNode ih3 = new IntNode(0);
            string source3 = BEncode.StringEncode(ih3);
            Assert.AreEqual(source3, "i0e");

            //Test4测试用例为-10
            IntNode ih4 = new IntNode(-10);
            string source4 = BEncode.StringEncode(ih4);
            Assert.AreEqual(source4, "i-10e");
        }

        /// <summary>
        /// 测试字节数组编码
        /// </summary>
        [Test]
        public void TestEncodeByteArray1()
        {
            //Test1标点符号
            BytesNode bah1 = new BytesNode("~!@#$%^&*()_+|`-=\\{}:\"<>?[];',./");
            string source1 = BEncode.StringEncode(bah1);
            Assert.AreEqual("32:~!@#$%^&*()_+|`-=\\{}:\"<>?[];',./", source1);

            //Test2空字符
            BytesNode bah2 = new BytesNode("");
            string source2 = BEncode.StringEncode(bah2);
            Assert.AreEqual(source2, "0:");

            //Test3英文字母与数字
            BytesNode bah3 = new BytesNode("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
            string source3 = BEncode.StringEncode(bah3);
            Assert.AreEqual(source3, "62:abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");

            //Test4中文字体与全角标点符号
            BytesNode bah4 = new BytesNode("微软公司，广州大学");
            string source4 = BEncode.StringEncode(bah4);
            Assert.AreEqual("18:微软公司，广州大学", source4);

            //Test5全角的数字与英文字母
            BytesNode bah5 = new BytesNode("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
            string source5 = BEncode.StringEncode(bah5);
            Assert.AreEqual("124:ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９", source5);
        }

        /// <summary>
        /// 测试列表编码
        /// </summary>
        [Test]
        public void TestEncodeList1()
        {
            //Test1
            ListNode list1 = new ListNode();
            string source1 = BEncode.StringEncode(list1);
            Assert.AreEqual("{}", source1);

            //Test2
            ListNode list2 = new ListNode();
            for (int i = 1; i <= 3; i++)
                list2.Add(i);
            string source2 = BEncode.StringEncode(list2);
            Assert.AreEqual(source2, "{i1ei2ei3e}");

            //Test3
            ListNode lh31 = new ListNode();
            lh31.Add("Alice");
            lh31.Add("Bob");
            ListNode lh32 = new ListNode();
            lh32.Add(2);
            lh32.Add(3);
            ListNode list3 = new ListNode(new List<BEncodedNode>(new BEncodedNode[] { lh31, lh32 }));
            string source3 = BEncode.StringEncode(list3);
            Assert.AreEqual(source3, "{{5:Alice3:Bob}{i2ei3e}}");
        }

        /// <summary>
        /// 测试字典编码
        /// </summary>
        [Test]
        public void TestEncodeDictionary1()
        {
            //Test1
            DictNode dict1 = new DictNode();
            string source1 = BEncode.StringEncode(dict1);
            Assert.AreEqual(source1, "[]");

            //Test2
            DictNode dict2 = new DictNode();
            dict2.Add("age", 25);
            dict2.Add("eyes", "blue");
            string source2 = BEncode.StringEncode(dict2);
            Assert.AreEqual(source2, "[3:agei25e4:eyes4:blue]");


            //Test3
            DictNode dh31 = new DictNode();
            dh31.Add(Encoding.Default.GetBytes("author"), "Alice");
            dh31.Add("length", 1048576);
            DictNode dict3 = new DictNode();
            dict3.Add("spam.mp3", dh31);
            string source3 = BEncode.StringEncode(dict3);
            Assert.AreEqual(source3, "[8:spam.mp3[6:author5:Alice6:lengthi1048576e]]");
            Assert.AreEqual(dict3.ToString(), "[8:spam.mp3[6:author5:Alice6:lengthi1048576e]]");
        }
        #endregion
    }
}
