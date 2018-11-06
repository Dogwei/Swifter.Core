using System;

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

        public Type GetDynamicProviderFactoryType()
        {
            var tConnection = this.tConnection ?? throw new NotSupportedException("Connection");
            var tCommand = this.tCommand ?? throw new NotSupportedException("Command");
            var tDataAdapter = this.tDataAdapter ?? typeof(object);
            var tParameter = this.tParameter ?? throw new NotSupportedException("Parameter");
            var tParameterCollection = this.tParameterCollection ?? throw new NotSupportedException("ParameterCollection");
            var tDataReader = this.tDataReader ?? throw new NotSupportedException("DataReader");
            var tTransaction = this.tTransaction ?? throw new NotSupportedException("Transaction");

            var type = typeof(DynamicProviderFactory<,,,,,,>);

            type = type.MakeGenericType(tConnection, tCommand, tDataAdapter, tParameter, tParameterCollection, tDataReader, tTransaction);

            return type;
        }
    }
}