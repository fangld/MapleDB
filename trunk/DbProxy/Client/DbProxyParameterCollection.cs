using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DbProxy.Client
{
    internal class DbProxyParameterCollection : DbParameterCollection
    {
        #region Fields

        private ArrayList parameters;
        private IDictionary<string, int> indexs;
        private DbProxyCommand command;

        #endregion

        #region Constructors

        public DbProxyParameterCollection(DbProxyCommand command)
        {
            this.command = command;
            parameters = new ArrayList();
            indexs = new Dictionary<string, int>();
        }

        #endregion

        #region Overriden IList

        public override int Add(object value)
        {
            DbParameter para = value as DbParameter;
            parameters.Add(para);
            indexs.Add(para.ParameterName, indexs.Count);
            return 1;
        }

        public override void AddRange(Array values)
        {
            foreach (object value in values)
            {
                Add(value);
            }
        }

        public override void Clear()
        {
            parameters.Clear();
        }

        public override bool Contains(string value)
        {
            return parameters.Contains(value);
        }

        public override bool Contains(object value)
        {
            return parameters.Contains(value);
        }

        public override void CopyTo(Array array, int index)
        {
            parameters.CopyTo(array, index);
        }

        public override int Count
        {
            get { return parameters.Count; }
        }

        public override IEnumerator GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        public override int IndexOf(object value)
        {
            return parameters.IndexOf(value);
        }

        public override void Insert(int index, object value)
        {
            parameters.Insert(index, value);
        }

        public override bool IsFixedSize
        {
            get { return parameters.IsFixedSize; }
        }

        public override bool IsReadOnly
        {
            get { return parameters.IsReadOnly; }
        }

        public override bool IsSynchronized
        {
            get { return parameters.IsSynchronized; }
        }

        public override void Remove(object value)
        {
            parameters.Remove(value);
        }

        public override void RemoveAt(int index)
        {
            parameters.RemoveAt(index);
        }

        public override object SyncRoot
        {
            get { return parameters.SyncRoot; }
        }

        #endregion

        #region Overriden ParameterCollection

        protected override DbParameter GetParameter(string parameterName)
        {
            int index = indexs[parameterName];
            return (DbParameter)parameters[index];
        }

        protected override DbParameter GetParameter(int index)
        {
            return (DbParameter)parameters[index];
        }

        public override int IndexOf(string parameterName)
        {
            return indexs[parameterName];
        }

        public override void RemoveAt(string parameterName)
        {
            int index = indexs[parameterName];
            parameters.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            int index = indexs[parameterName];
            parameters[index] = value;
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            parameters[index] = value;
        }

        #endregion
    }
}
