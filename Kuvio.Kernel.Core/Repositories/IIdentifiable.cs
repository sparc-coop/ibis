using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Core
{
    public interface IIdentifiable
    {
        string Id { get; set; }
    }
}