using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net.Sockets;
using System.Text;

namespace DbProxy.Client
{
    /// <summary>
    /// Db Proxy参数
    /// </summary>
    public class DbProxyParameter : DbParameter, ICloneable
    {
        #region Fields

        private DbType dbType;
        private ParameterDirection direction;
        private bool isNullable;
        private string parameterName;
        private int size;
        private string sourceColumn;
        private bool sourceColumnNullMapping;
        private DataRowVersion sourceVersion;
        private object value;

        #endregion

        #region Properties

        public override DbType DbType
        {
            get
            {
                return dbType;
            }
            set
            {
                dbType = value;
            }
        }

        public override ParameterDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return isNullable;
            }
            set
            {
                isNullable = value;
            }
        }

        public override string ParameterName
        {
            get
            {
                return parameterName;
            }
            set
            {
                parameterName = value;
            }
        }

        public override int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        public override string SourceColumn
        {
            get
            {
                return sourceColumn;
            }
            set
            {
                sourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                return sourceColumnNullMapping;
            }
            set
            {
                sourceColumnNullMapping = value;
            }
        }

        public override DataRowVersion SourceVersion
        {
            get
            {
                return sourceVersion;
            }
            set
            {
                sourceVersion = value;
            }
        }

        public override object Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        #endregion

        #region Constructors

        public DbProxyParameter()
            : this(string.Empty, DbType.AnsiString, 0)
        { }

        public DbProxyParameter(string parameterName, DbType dbType)
            : this(parameterName, dbType, 0)
        { }

        public DbProxyParameter(string parameterName, DbType dbType, int size)
        {
            this.parameterName = parameterName;
            this.size = size;
            this.dbType = dbType;
            direction = ParameterDirection.Input;
        }


        #endregion

        #region Methods

        public override void ResetDbType()
        {
            dbType = DbType.AnsiString;
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            DbProxyParameter result = new DbProxyParameter(parameterName, dbType, size);
            result.direction = direction;
            return result;
        }

        #endregion
    }
}
