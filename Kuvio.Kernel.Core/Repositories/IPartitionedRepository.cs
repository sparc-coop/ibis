namespace Kuvio.Kernel.Core
{
    public interface IPartitionedRepository<T> : IRepository<T>
    {
        IPartitionedRepository<T> Partition(string partitionKey);
    }
}