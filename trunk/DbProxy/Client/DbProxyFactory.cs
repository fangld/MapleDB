using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security;
using System.Text;

namespace DbProxy.Client
{
    /// <summary>
    /// Db Proxy生成工厂
    /// </summary>
    public class DbProxyFactory : DbProviderFactory
    {
        #region Fields

        /// <summary>
        /// 单件实例
        /// </summary>
        public static readonly DbProxyFactory Instance;

        #endregion

        #region Properties

        /// <summary>
        /// 如果 DbProviderFactory 的实例支持 DbDataSourceEnumerator 类，则为 true；否则为 false。
        /// </summary>
        public override bool CanCreateDataSourceEnumerator
        {
            get { return false; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private DbProxyFactory()
        {
        }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static DbProxyFactory()
        {
            Instance = new DbProxyFactory();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 返回实现 DbCommand 类的提供程序的类的一个新实例。
        /// </summary>
        /// <returns>DbCommand 的新实例。 </returns>
        public override DbCommand CreateCommand()
        {

            return new DbProxyCommand();
        }

        /// <summary>
        /// 返回实现 DbCommandBuilder 类的提供程序的类的一个新实例。 
        /// </summary>
        /// <returns>DbCommandBuilder 的新实例。 </returns>
        public override DbCommandBuilder CreateCommandBuilder()
        {
            throw new NotSupportedException();
            //return new DbProxyCommandBuilder();
        }

        /// <summary>
        /// 返回实现 DbConnection 类的提供程序的类的一个新实例。 
        /// </summary>
        /// <returns>DbConnection 的新实例。</returns>
        public override DbConnection CreateConnection()
        {
            return new DbProxyConnection();
        }

        /// <summary>
        /// 返回实现 DbConnectionStringBuilder 类的提供程序的类的一个新实例。 
        /// </summary>
        /// <returns>DbConnectionStringBuilder 的新实例。 </returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            throw new NotSupportedException();
            //return new SqlConnectionStringBuilder();
        }

        /// <summary>
        /// 返回实现 DbDataAdapter 类的提供程序的类的一个新实例。
        /// </summary>
        /// <returns>DbDataAdapter 的新实例。 </returns>
        public override DbDataAdapter CreateDataAdapter()
        {
            throw new NotSupportedException();
            //return new SqlDataAdapter();
        }

        /// <summary>
        /// 返回实现 DbDataSourceEnumerator 类的提供程序的类的一个新实例。 
        /// </summary>
        /// <returns>DbDataSourceEnumerator 的新实例。 </returns>
        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            throw new NotSupportedException();
            //return SqlDataSourceEnumerator.Instance;
        }

        /// <summary>
        /// 返回实现 DbParameter 类的提供程序的类的一个新实例。 
        /// </summary>
        /// <returns>DbParameter 的新实例。 </returns>
        public override DbParameter CreateParameter()
        {
            return new DbProxyParameter();
        }

        /// <summary>
        /// 返回提供程序的类的新实例，该实例可实现提供程序的 CodeAccessPermission 类的版本。 
        /// </summary>
        /// <param name="state">PermissionState 值之一。</param>
        /// <returns>指定 PermissionState 的 CodeAccessPermission 对象。 </returns>
        public override CodeAccessPermission CreatePermission(System.Security.Permissions.PermissionState state)
        {
            throw new NotSupportedException();
            //return base.CreatePermission(state);
        }

        #endregion
    }
}
