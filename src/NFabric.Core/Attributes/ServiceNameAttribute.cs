namespace NFabric.Core.Attributes
{
    using System;

    /// <summary>
    /// Provides name for service resolution using <see cref="IServiceProvider"/> and extension methods from <see cref="Extensions.ServiceCollectionExtensions"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ServiceNameAttribute : Attribute
    {
        public ServiceNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
