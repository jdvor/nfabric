namespace NFabric.Core.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class ExceptionExtensions
    {
        public static Exception InnermostException(this Exception ex)
        {
            Exception e = ex;
            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            return e;
        }

        public static string Describe(this Exception ex)
        {
            var typeNames = new List<string>(4) { ex.GetType().Name };
            Exception e = ex;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                typeNames.Add(e.GetType().Name);
            }

            if (typeNames.Count == 1)
            {
                return $"{ex.Message} ({typeNames[0]})";
            }

            typeNames.Reverse();
            return $"{e.Message} ({typeNames.MakeString(" -> ")})";
        }
    }
}
