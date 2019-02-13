using Swifter.Tools;
using System;
using System.Data;

namespace Swifter.Data
{
    class ProviderClasses
    {
        public Type tConnection;
        public Type tCommand;
        public Type tDataAdapter;
        public Type tParameter;
        public Type tParameterCollection;
        public Type tDataReader;
        public Type tTransaction;

        public string tProviderName;

        public ProviderClasses(string tProviderName)
        {
            this.tProviderName = tProviderName;
        }

        public Type GetDynamicProviderFactoryType()
        {
            var tConnection = this.tConnection ?? throw new NotSupportedException(StringHelper.Format("No Type Implement '{1}' In Package '{0}'.", tProviderName, typeof(IDbConnection).FullName));
            var tCommand = this.tCommand ?? throw new NotSupportedException(StringHelper.Format("No Type Implement '{1}' In Package '{0}'.", tProviderName, typeof(IDbCommand).FullName));
            var tDataAdapter = this.tDataAdapter ?? typeof(object);
            var tParameter = this.tParameter ?? throw new NotSupportedException(StringHelper.Format("No Type Implement '{1}' In Package '{0}'.", tProviderName, typeof(IDataParameter).FullName));
            var tParameterCollection = this.tParameterCollection ?? throw new NotSupportedException(StringHelper.Format("No Type Implement '{1}' In Package '{0}'.", tProviderName, typeof(IDataParameterCollection).FullName));
            var tDataReader = this.tDataReader ?? throw new NotSupportedException(StringHelper.Format("No Type Implement '{1}' In Package '{0}'.", tProviderName, typeof(IDataReader).FullName));
            var tTransaction = this.tTransaction ?? throw new NotSupportedException(StringHelper.Format("No Type Implement '{1}' In Package '{0}'.", tProviderName, typeof(IDbTransaction).FullName));

            var type = typeof(ProxyProviderFactory<,,,,,,>);

            type = type.MakeGenericType(tConnection, tCommand, tDataAdapter, tParameter, tParameterCollection, tDataReader, tTransaction);

            return type;
        }
    }
}