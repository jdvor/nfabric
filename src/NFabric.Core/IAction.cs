namespace NFabric.Core
{
    using Microsoft.Extensions.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Named action asynchronuosly executed. Used in console applications to execute particular task
    /// based on the name usually passed from outside as command line parameter.
    /// </summary>
    public interface IAction
    {
        Task ExecuteAsync(IConfiguration config, CancellationToken cancellationToken = default(CancellationToken));

        string Name { get; }
    }
}
