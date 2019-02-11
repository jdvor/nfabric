namespace NFabric.Core.Serialization
{
    using JetBrains.Annotations;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class TypeIdSerializationBinder : DefaultSerializationBinder
    {
        private const string AssemblyIgnorePlaceholder = "_";

        private static readonly Regex AutoMapperRx = new Regex(
            @"Proxy<(?<typeName>[a-z][a-z0-9\.]+)_(?<assembly>[a-z][a-z0-9\.]+)_",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly TypeIdTranslator translator;

        public TypeIdSerializationBinder(IEnumerable<Type> initializeTypes = null)
        {
            translator = new TypeIdTranslator(initializeTypes);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType.Assembly.GetName().Name == "AutoMapper.Proxies")
            {
                var m = AutoMapperRx.Match(serializedType.FullName);
                if (m.Success)
                {
                    var iName = m.Groups["typeName"].Value;
                    var iType = serializedType.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t => t.FullName == iName);
                    if (iType != null)
                    {
                        serializedType = iType;
                    }
                }
            }

            var typeId = translator.Translate(serializedType);
            assemblyName = AssemblyIgnorePlaceholder;
            typeName = typeId;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName == AssemblyIgnorePlaceholder)
            {
                return translator.Translate(typeName);
            }

            if (assemblyName == "AutoMapper.Proxies")
            {
                var m = AutoMapperRx.Match(typeName);
                if (m.Success)
                {
                    assemblyName = m.Groups["assembly"].Value;
                    typeName = m.Groups["typeName"].Value;
                }
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}
