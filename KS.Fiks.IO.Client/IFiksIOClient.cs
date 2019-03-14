using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client
{
    public interface IFiksIOClient
    {
        string AccountId { get; }

        Task<Account> Lookup(LookupRequest request);
    }
}