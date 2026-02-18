using AgroSolutions.Ingestion.Service.Application.Dtos.Weather;

namespace AgroSolutions.Ingestion.Service.Application.Interfaces;

/// <summary>
/// Interface para serviço de previsão do tempo
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Obtém dados de previsão do tempo para uma localização aleatória
    /// </summary>
    Task<PrevisaoClimaDto> ObterPrevisaoAleatoriaAsync();
}
