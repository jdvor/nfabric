namespace NFabric.Core.Extensions
{
    using System;

    public static class ExpressionExtensions
    {
        public static void ExecuteRepeatedly(this Action action, int count)
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
    }
}
