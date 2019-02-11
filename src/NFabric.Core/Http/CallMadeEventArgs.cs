namespace NFabric.Core.Http
{
    using System;

    public class CallMadeEventArgs : EventArgs
    {
        public CallMadeEventArgs(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
