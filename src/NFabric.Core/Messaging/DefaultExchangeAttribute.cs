namespace NFabric.Core.Messaging
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultExchangeAttribute : Attribute
    {
        public DefaultExchangeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
