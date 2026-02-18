using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;
using AgroSolutions.Ingestion.Service.Application.Dtos.Weather;

namespace AgroSolutions.Ingestion.Service.Application.Interfaces;

public interface IMongoDbService
{
    Task SalvarDadosSensorAsync(ReceberDadosSensorDto dto, PrevisaoClimaDto? previsaoClima = null);
    Task<List<ReceberDadosSensorDto>> ObterDadosPorTalhaoAsync(Guid talhaoId, DateTime? dataInicio = null, DateTime? dataFim = null);
}
