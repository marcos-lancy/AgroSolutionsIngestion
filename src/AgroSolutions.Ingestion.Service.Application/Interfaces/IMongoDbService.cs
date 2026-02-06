using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;

namespace AgroSolutions.Ingestion.Service.Application.Interfaces;

public interface IMongoDbService
{
    Task SalvarDadosSensorAsync(ReceberDadosSensorDto dto);
    Task<List<ReceberDadosSensorDto>> ObterDadosPorTalhaoAsync(Guid talhaoId, DateTime? dataInicio = null, DateTime? dataFim = null);
}
