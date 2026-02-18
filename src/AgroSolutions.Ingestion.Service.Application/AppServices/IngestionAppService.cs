using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;
using AgroSolutions.Ingestion.Service.Application.Dtos.Weather;
using AgroSolutions.Ingestion.Service.Application.Interfaces;
using AgroSolutions.Ingestion.Service.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Ingestion.Service.Application.AppServices;

public class IngestionAppService : IIngestionAppService
{
    private readonly IMongoDbService _mongoDbService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IWeatherService _weatherService;
    private readonly ILogger<IngestionAppService> _logger;

    public IngestionAppService(
        IMongoDbService mongoDbService,
        IPublishEndpoint publishEndpoint,
        IWeatherService weatherService,
        ILogger<IngestionAppService> logger)
    {
        _mongoDbService = mongoDbService;
        _publishEndpoint = publishEndpoint;
        _weatherService = weatherService;
        _logger = logger;
    }

    public async Task ReceberDadosSensorAsync(ReceberDadosSensorDto dto)
    {
        _logger.LogInformation("Recebendo dados de sensor para talhão {TalhaoId}", dto.TalhaoId);

        // Verificar se os dados do sensor estão preenchidos
        // Se não estiverem, buscar previsão do tempo como fallback
        PrevisaoClimaDto? previsaoClima = null;
        bool dadosDoSensor = dto.Temperatura != 0 || dto.UmidadeSolo != 0;

        if (!dadosDoSensor)
        {
            previsaoClima = await _weatherService.ObterPrevisaoAleatoriaAsync();

            _logger.LogInformation(
                "Dados do sensor não preenchidos - usando previsão do tempo: {Local} - Temp: {Temp}°C, Umidade: {Umidade}%",
                previsaoClima.NomeLocalizacao,
                previsaoClima.Temperatura,
                previsaoClima.UmidadeRelativa);
        }

        await _mongoDbService.SalvarDadosSensorAsync(dto, previsaoClima);

        var evento = new DadosSensorRecebidosEvent
        {
            TalhaoId = dto.TalhaoId,
            UmidadeSolo = dto.UmidadeSolo,
            Temperatura = dto.Temperatura,
            Precipitacao = dto.Precipitacao
        };

        await _publishEndpoint.Publish(evento);

        _logger.LogInformation("Dados de sensor processados e evento publicado para talhão {TalhaoId}", dto.TalhaoId);
    }
}
