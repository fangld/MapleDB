using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using DbProxy.Client;

namespace TestDbProxy
{
    public static class DbProxyDal
    {
        #region SqlServer strings

        private const string SqlInsertTestDbProxy = "t_InsertTestDbProxy";
        private const string SqlUpdateTestDbProxy = "t_UpdateTestDbProxy";
        private const string SqlDeleteTestDbProxy = "t_DeleteTestDbProxy";
        private const string SqlQueryTestDbProxy = "t_QueryTestDbProxy";
        private const string SqlQueryTestDbProxyOutput = "t_QueryTestDbProxyOutput";
        private const string SqlQueryTestDbProxyCount = "t_QueryTestDbProxyCount";
        private const string SqlAccessTimeout = "t_AccessTimeout";

        private const string proxyConnString = "Data Source=localhost;Device ID=sa;Priorty=2;Timeout=20000";

        #endregion

        #region Methods

        public static void DeleteText(string PK)
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("DELETE FROM TestDbProxy WHERE PK = '{0}'", PK);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Dispose();
        }

        public static void InsertTextNormal(string PK, long bigIntValue, int intValue, short smallIntValue, byte tinyIntValue, DateTime dateTimeValue, float realValue, double floatValue, string varcharText, string charText, byte[] varbinaryStream, byte[] binaryStream)
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                    string.Format(
                        "INSERT TestDbProxy VALUES('{0}', {1}, {2}, {3}, {4}, '{5}', {6}, {7}, '{8}', '{9}', {10}, {11})", PK,
                        bigIntValue, intValue, smallIntValue, tinyIntValue, dateTimeValue, realValue, floatValue,
                        varcharText, charText, BytesToString(binaryStream), BytesToString(varbinaryStream));
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Dispose();
        }

        public static void InsertTextNull(string PK)
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("INSERT TestDbProxy VALUES('{0}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)", PK);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Dispose();
        }

        public static void UpdateTextNormal(string PK, long bigIntValue, int intValue, short smallIntValue, byte tinyIntValue, DateTime dateTimeValue, float realValue, double floatValue, string varcharText, string charText, byte[] varbinaryStream, byte[] binaryStream)
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                    string.Format(
                        "UPDATE TestDbProxy SET BigIntValue = {1}, IntValue = {2}, SmallIntValue = {3}, TinyIntValue = {4}, DateTimeValue = '{5}', RealValue = {6}, FloatValue = {7}, VarcharText = '{8}', CharText = '{9}', VarbinaryStream = {10}, BinaryStream = {11} WHERE PK = '{0}'",
                        PK, bigIntValue, intValue, smallIntValue, tinyIntValue, dateTimeValue, realValue, floatValue,
                        varcharText, charText, BytesToString(binaryStream), BytesToString(varbinaryStream));
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Dispose();
        }

        public static void UpdateTextNull(string PK)
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                    string.Format(
                        "UPDATE TestDbProxy SET BigIntValue = NULL, IntValue = NULL, SmallIntValue = NULL, TinyIntValue = NULL, DateTimeValue = NULL, RealValue = NULL, FloatValue = NULL, VarcharText = NULL, CharText = NULL, VarbinaryStream = NULL, BinaryStream = NULL WHERE PK = '{0}'",
                        PK);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Dispose();
        }

        public static void QueryTextNormalByReader(string PK, out long bigIntValue, out int intValue, out short smallIntValue, out byte tinyIntValue, out DateTime dateTimeValue, out float realValue, out double floatValue, out string varcharText, out string charText, out byte[] varbinaryStream, out byte[] binaryStream)
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("SELECT * FROM TestDbProxy WHERE PK = '{0}'", PK);
                using (DbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    rdr.Read();
                    bigIntValue = rdr.GetInt64(1);
                    intValue = rdr.GetInt32(2);
                    smallIntValue = rdr.GetInt16(3);
                    tinyIntValue = rdr.GetByte(4);
                    dateTimeValue = rdr.GetDateTime(5);
                    realValue = rdr.GetFloat(6);
                    floatValue = rdr.GetDouble(7);
                    varcharText = rdr.GetString(8);
                    charText = rdr.GetString(9);
                    varbinaryStream = (byte[])rdr.GetValue(10);
                    binaryStream = (byte[])rdr.GetValue(11);
                }
            }
            cmd.Dispose();
        }

        public static void QueryTextNullByReader(string PK, out bool bigIntNull, out bool intNull, out bool smallIntNull, out bool tinyIntNull, out bool dateTimeNull, out bool realNull, out bool floatNull, out bool varcharNull, out bool charNull, out bool varbinaryNull, out bool binaryNull)
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("SELECT * FROM TestDbProxy WHERE PK = '{0}'", PK);
                using (DbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    rdr.Read();
                    bigIntNull = rdr.IsDBNull(1);
                    intNull = rdr.IsDBNull(2);
                    smallIntNull = rdr.IsDBNull(3);
                    tinyIntNull = rdr.IsDBNull(4);
                    dateTimeNull = rdr.IsDBNull(5);
                    realNull = rdr.IsDBNull(6);
                    floatNull = rdr.IsDBNull(7);
                    varcharNull = rdr.IsDBNull(8);
                    charNull = rdr.IsDBNull(9);
                    varbinaryNull = rdr.IsDBNull(10);
                    binaryNull = rdr.IsDBNull(11);
                }
            }
            cmd.Dispose();
        }

        public static void InsertProcNormal(string PK, long bigIntValue, int intValue, short smallIntValue, byte tinyIntValue, DateTime dateTimeValue, float realValue, double floatValue, string varcharText, string charText, byte[] varbinaryStream, byte[] binaryStream)
        {
            DbParameter[] parms = GetParameters();
            parms[0].Value = PK;
            parms[1].Value = bigIntValue;
            parms[2].Value = intValue;
            parms[3].Value = smallIntValue;
            parms[4].Value = tinyIntValue;
            parms[5].Value = dateTimeValue;
            parms[6].Value = realValue;
            parms[7].Value = floatValue;
            parms[8].Value = varcharText;
            parms[9].Value = charText;
            parms[10].Value = varbinaryStream;
            parms[11].Value = binaryStream;
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.AddRange(parms);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlInsertTestDbProxy;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void InsertProcNull(string PK)
        {
            DbParameter[] parms = GetParameters();
            parms[0].Value = PK;
            parms[1].Value = DBNull.Value;
            parms[2].Value = DBNull.Value;
            parms[3].Value = DBNull.Value;
            parms[4].Value = DBNull.Value;
            parms[5].Value = DBNull.Value;
            parms[6].Value = DBNull.Value;
            parms[7].Value = DBNull.Value;
            parms[8].Value = DBNull.Value;
            parms[9].Value = DBNull.Value;
            parms[10].Value = DBNull.Value;
            parms[10].Value = DBNull.Value;
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.AddRange(parms);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlInsertTestDbProxy;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void UpdateProcNormal(string PK, long bigIntValue, int intValue, short smallIntValue, byte tinyIntValue, DateTime dateTimeValue, float realValue, double floatValue, string varcharText, string charText, byte[] varbinaryStream, byte[] binaryStream)
        {
            DbParameter[] parms = GetParameters();
            parms[0].Value = PK;
            parms[1].Value = bigIntValue;
            parms[2].Value = intValue;
            parms[3].Value = smallIntValue;
            parms[4].Value = tinyIntValue;
            parms[5].Value = dateTimeValue;
            parms[6].Value = realValue;
            parms[7].Value = floatValue;
            parms[8].Value = varcharText;
            parms[9].Value = charText;
            parms[10].Value = varbinaryStream;
            parms[11].Value = binaryStream;
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.AddRange(parms);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlUpdateTestDbProxy;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void UpdateProcNull(string PK)
        {
            DbParameter[] parms = GetParameters();
            parms[0].Value = PK;
            parms[1].Value = DBNull.Value;
            parms[2].Value = DBNull.Value;
            parms[3].Value = DBNull.Value;
            parms[4].Value = DBNull.Value;
            parms[5].Value = DBNull.Value;
            parms[6].Value = DBNull.Value;
            parms[7].Value = DBNull.Value;
            parms[8].Value = DBNull.Value;
            parms[9].Value = DBNull.Value;
            parms[10].Value = DBNull.Value;
            parms[11].Value = DBNull.Value;
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.AddRange(parms);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlUpdateTestDbProxy;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void DeleteProc(string PK)
        {
            DbParameter parm = new DbProxyParameter("@PK", DbType.AnsiString, 50);
            parm.Value = PK;
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.Add(parm);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlDeleteTestDbProxy;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            cmd.Dispose();
        }

        public static void QueryProcNormalByReader(string PK, out long bigIntValue, out int intValue, out short smallIntValue, out byte tinyIntValue, out DateTime dateTimeValue, out float realValue, out double floatValue, out string varcharText, out string charText, out byte[] varbinaryStream, out byte[] binaryStream)
        {
            DbParameter parm = new DbProxyParameter("@PK", DbType.AnsiString, 50);
            parm.Value = PK;
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.Add(parm);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlQueryTestDbProxy;
                using (DbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    rdr.Read();
                    bigIntValue = rdr.GetInt64(1);
                    intValue = rdr.GetInt32(2);
                    smallIntValue = rdr.GetInt16(3);
                    tinyIntValue = rdr.GetByte(4);
                    dateTimeValue = rdr.GetDateTime(5);
                    realValue = rdr.GetFloat(6);
                    floatValue = rdr.GetDouble(7);
                    varcharText = rdr.GetString(8);
                    charText = rdr.GetString(9);
                    varbinaryStream = (byte[])rdr.GetValue(10);
                    binaryStream = (byte[])rdr.GetValue(11);
                }
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void QueryProcNormalByScalar(string PK, out int linenum, out int returnValue)
        {
            DbParameter returnParm = new DbProxyParameter("@RETURN_VALUE", DbType.Int32);
            returnParm.Direction = ParameterDirection.ReturnValue;
            DbParameter pkParm = new DbProxyParameter("@PK", DbType.AnsiString, 50);
            pkParm.Value = PK;
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.Add(returnParm);
            cmd.Parameters.Add(pkParm);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlQueryTestDbProxyCount;
                linenum = (int)cmd.ExecuteScalar();
            }
            returnValue = (int)returnParm.Value;
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void QueryProcNormalByOutput(string PK, out long bigIntValue, out int intValue, out short smallIntValue, out byte tinyIntValue, out DateTime dateTimeValue, out float realValue, out double floatValue, out string varcharText, out string charText, out byte[] varbinaryStream, out byte[] binaryStream)
        {
            DbParameter[] parms = GetParameters();
            parms[0].Value = PK;
            for (int i = 1; i < parms.Length; i++)
            {
                parms[i].Direction = ParameterDirection.Output;
            }
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.AddRange(parms);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlQueryTestDbProxyOutput;
                cmd.ExecuteNonQuery();
            }
            bigIntValue = (long) parms[1].Value;
            intValue = (int) parms[2].Value;
            smallIntValue = (short) parms[3].Value;
            tinyIntValue = (byte) parms[4].Value;
            dateTimeValue = (DateTime) parms[5].Value;
            realValue = (float) parms[6].Value;
            floatValue = (double) parms[7].Value;
            varcharText = (string) parms[8].Value;
            charText = (string) parms[9].Value;
            varbinaryStream = (byte[]) parms[10].Value;
            binaryStream = (byte[]) parms[11].Value;

            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void QueryProcNullByReader(string PK, out bool bigIntNull, out bool intNull, out bool smallIntNull, out bool tinyIntNull, out bool dateTimeNull, out bool realNull, out bool floatNull, out bool varcharNull, out bool charNull, out bool varbinaryNull, out bool binaryNull)
        {
            DbParameter parm = new DbProxyParameter("@PK", DbType.AnsiString, 50);
            parm.Value = PK; 
            DbCommand cmd = new DbProxyCommand();
            cmd.Parameters.Add(parm);
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlQueryTestDbProxy;
                using (DbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    rdr.Read();
                    bigIntNull = rdr.IsDBNull(1);
                    intNull = rdr.IsDBNull(2);
                    smallIntNull = rdr.IsDBNull(3);
                    tinyIntNull = rdr.IsDBNull(4);
                    dateTimeNull = rdr.IsDBNull(5);
                    realNull = rdr.IsDBNull(6);
                    floatNull = rdr.IsDBNull(7);
                    varcharNull = rdr.IsDBNull(8);
                    charNull = rdr.IsDBNull(9);
                    varbinaryNull = rdr.IsDBNull(10);
                    binaryNull = rdr.IsDBNull(11);
                }
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        public static void AccessTimeout()
        {
            DbCommand cmd = new DbProxyCommand();
            using (DbConnection conn = new DbProxyConnection())
            {
                conn.ConnectionString = proxyConnString;
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlAccessTimeout;
                cmd.ExecuteNonQuery();
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        private static string BytesToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2 + 2);
            sb.Append("0X");
            for (int i = 0; i < bytes.Length; i++)
            {
                string str = bytes[i] < 16 ? string.Format("0{0:X}", bytes[i]) : string.Format("{0:X}", bytes[i]);
                sb.Append(str);
            }
            return sb.ToString();
        }

        private static DbParameter[] GetParameters()
        {
            DbParameter[] result = new DbProxyParameter[]
                                       {
                                           new DbProxyParameter("@PK", DbType.AnsiString, 50),
                                           new DbProxyParameter("@BigIntValue", DbType.Int64),
                                           new DbProxyParameter("@IntValue", DbType.Int32),
                                           new DbProxyParameter("@SmallIntValue", DbType.Int16),
                                           new DbProxyParameter("@TinyIntValue", DbType.Byte),
                                           new DbProxyParameter("@DateTimeValue", DbType.DateTime),
                                           new DbProxyParameter("@RealValue", DbType.Single),
                                           new DbProxyParameter("@FloatValue", DbType.Double),
                                           new DbProxyParameter("@VarcharText", DbType.AnsiString, 50),
                                           new DbProxyParameter("@CharText", DbType.AnsiStringFixedLength, 10),
                                           new DbProxyParameter("@VarbinaryStream", DbType.Binary, 50),
                                           new DbProxyParameter("@BinaryStream", DbType.Binary, 50),
                                       };
            return result;
        }

        #endregion
    }
}