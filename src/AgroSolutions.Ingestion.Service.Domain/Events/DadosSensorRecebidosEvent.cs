namespace AgroSolutions.Ingestion.Service.Domain.Events;

public record DadosSensorRecebidosEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid TalhaoId { get; init; }
    public decimal UmidadeSolo { get; init; }
    public decimal Temperatura { get; init; }
    public decimal Precipitacao { get; init; }
}
