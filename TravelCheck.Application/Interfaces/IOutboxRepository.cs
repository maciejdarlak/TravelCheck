using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelCheck.Domain.Entities;

namespace TravelCheck.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxEvent evt, CancellationToken ct);
        Task<IEnumerable<OutboxEvent>> GetUnprocessedAsync();
        Task MarkProcessedAsync(Guid id, string type);
    }
}
