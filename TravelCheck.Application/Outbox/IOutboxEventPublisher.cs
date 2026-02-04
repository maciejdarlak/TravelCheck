using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelCheck.Application.Outbox;

public interface IOutboxEventPublisher
{
    Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default);
}
