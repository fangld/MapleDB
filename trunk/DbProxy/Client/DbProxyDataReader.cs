using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using DbProxy.DbProxyTypes;

namespace DbProxy.Client
{
    public class DbProxyDataReader : DbDataReader
    {
        #region Fields

        private DbProxyCommand command;
        private ArrayList dataTypeNames;
        private bool disposed;
        private bool isClosed;
        private bool moreResults;
        private int resultsRead;
        private int rowsRead;
        private DataTable schemaTable;
        private bool haveRead;
        private bool readResult;
        private bool readResultUsed;
        private int currentIndex;
        private DataRow currentRow;
        private int visibleFieldCount;

        #endregion

        #region Properties

        public override int Depth
        {
            get { return 0; }
        }

        public override int FieldCount
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasRows
        {
            get
            {
                if (haveRead)
                    return readResult;
                haveRead = true;
                throw new NotImplementedException();
                return readResult;
            }
        }

        public override bool IsClosed
        {
            get { return isClosed; }
        }

        public override int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        internal DataTable SchemaTable
        {
            get { return schemaTable; }
        }

        #endregion

        #region Constructors

        internal DbProxyDataReader(DbProxyCommand command)
        {
            readResult = false;
            haveRead = false;
            readResultUsed = false;
            this.command = command;
            resultsRead = 0;
            isClosed = false;
            visibleFieldCount = 0;
            schemaTable = new DataTable();
            currentIndex = 0;
        }

        #endregion

        #region Methods

        new void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (schemaTable != null)
                        schemaTable.Dispose();
                    Close();
                    command = null;
                }
                disposed = true;
            }
        }

        private void ValidateState()
        {
            if (IsClosed)
                throw new InvalidOperationException("Invalid attempt to read data when reader is closed");
        }

        public override void Close()
        {
            if (isClosed)
                return;
            while (NextResult()) ;
            isClosed = true;
            command.CloseDataReader(moreResults);
        }

        public override bool GetBoolean(int ordinal)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override byte GetByte(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is byte))
            {
                if (value is DBNull) throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType());
            }
            return (byte)value;
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override DateTime GetDateTime(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is DateTime))
            {
                if (value is DBNull)
                    throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType());
            }
            return (DateTime)value;
        }

        public override decimal GetDecimal(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is decimal))
            {
                if (value is DBNull)
                    throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType());
            }
            return (decimal)value;
        }

        public override double GetDouble(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is double))
            {
                if (value is DBNull)
                    throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType());
            }
            return (double)value;
        }

        public override IEnumerator GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override Type GetFieldType(int ordinal)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override float GetFloat(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is float))
            {
                if (value is DBNull)
                    throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType());
            }
            return (float)value;
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override short GetInt16(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is short))
            {
                if (value is DBNull) throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType().ToString());
            }
            return (short)value;
        }

        public override int GetInt32(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is int))
            {
                if (value is DBNull) throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType().ToString());
            }
            return (int)value;
        }

        public override long GetInt64(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is long))
            {
                if (value is DBNull) throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType().ToString());
            }
            return (long)value;
        }

        public override string GetName(int ordinal)
        {
            //if (ordinal < 0 || ordinal >= command.Tds.Columns.Count)
            //    throw new IndexOutOfRangeException();
            //return (string)command.Tds.Columns[ordinal].ColumnName;
            throw new NotImplementedException();
        }

        public override int GetOrdinal(string name)
        {
            //string colName;
            //foreach (DataColumn schema in command.Tds.Columns)
            //{
            //    colName = schema.ColumnName;
            //    if (colName.Equals(name) || string.Compare(colName, name, true) == 0)
            //        return schema.Ordinal;
            //}
            //throw new IndexOutOfRangeException();
            throw new NotImplementedException();
        }

        public override DataTable GetSchemaTable()
        {
            return schemaTable;
        }

        public override string GetString(int ordinal)
        {
            object value = GetValue(ordinal);
            if (!(value is string))
            {
                if (value is DBNull) throw new DbProxyNullValueException();
                throw new InvalidCastException("Type is " + value.GetType());
            }
            return (string)value;
        }

        public override object GetValue(int ordinal)
        {
            //Console.WriteLine("SchemaTable Count:{0}", schemaTable.Columns.Count);
            if (ordinal < 0 || ordinal >= schemaTable.Columns.Count)
                throw new IndexOutOfRangeException();
            return currentRow[ordinal];
        }

        public override int GetValues(object[] values)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool IsDBNull(int ordinal)
        {
            //Console.WriteLine("index:{0}, DBNull:{1}, Value:{2}", ordinal, GetValue(ordinal) == null, ((string)GetValue(ordinal)).Length);
            return GetValue(ordinal) == DBNull.Value;
        }

        public override bool NextResult()
        {
            if (currentIndex >= schemaTable.Rows.Count)
            {
                return false;
            }

            currentIndex++;

            return true;
        }

        public override bool Read()
        {
            if (currentIndex >= schemaTable.Rows.Count)
            {
                return false;
            }

            currentRow = schemaTable.Rows[currentIndex];
            currentIndex++;
            return true;
        }

        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public override object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }

        #endregion
    }
}
