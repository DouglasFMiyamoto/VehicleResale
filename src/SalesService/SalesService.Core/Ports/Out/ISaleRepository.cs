using SalesService.Core.Domain.Entities;

namespace SalesService.Core.Ports.Out;

public interface ISaleRepository
{
    Task CreateAsync(Sale sale, CancellationToken ct);

    Task<Sale?> GetByIdAsync(string saleId, CancellationToken ct);

    /// <summary>
    /// Idempotência para a SAGA: se já existe venda para ReservationId, retorna ela.
    /// </summary>
    Task<Sale?> GetByReservationIdAsync(string reservationId, CancellationToken ct);

    Task<IReadOnlyList<Sale>> ListAllAsync(CancellationToken ct);
}