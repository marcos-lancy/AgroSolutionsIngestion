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
    private readonly IBus _bus;
    private readonly IWeatherService _weatherService;
    private readonly ILogger<IngestionAppService> _logger;

    public IngestionAppService(
        IMongoDbService mongoDbService,
        IBus bus,
        IWeatherService weatherService,
        ILogger<IngestionAppService> logger)
    {
        _mongoDbService = mongoDbService;
        _bus = bus;
        _weatherService = weatherService;
        _logger = logger;
    }

    public async Task ReceberDadosSensorAsync(ReceberDadosSensorDto dto)
    {
        _logger.LogInformation("Recebendo dados de sensor para talhao {TalhaoId}", dto.TalhaoId);

        // Verificar se os dados do sensor estao preenchidos
        // Se nao estiverem, buscar previsao do tempo como fallback
        PrevisaoClimaDto? previsaoClima = null;
        bool dadosDoSensor = dto.Temperatura != 0 || dto.UmidadeSolo != 0;

        if (!dadosDoSensor)
        {
            previsaoClima = await _weatherService.ObterPrevisaoAleatoriaAsync();

            _logger.LogInformation(
                "Dados do sensor nao preenchidos - usando previsao do tempo: {Local} - Temp: {Temp}C, Umidade: {Umidade}%",
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

        // Publicar evento para o worker
        _logger.LogInformation("===============================================");
        _logger.LogInformation("PUBLICANDO EVENTO PARA O RABBITMQ...");
        _logger.LogInformation("Tipo do evento: {EventType}", typeof(DadosSensorRecebidosEvent).FullName);
        _logger.LogInformation("===============================================");
        await _bus.Publish(evento);
        _logger.LogInformation("Evento publicado com sucesso!");

        _logger.LogInformation("Dados de sensor processados e publicados para o barramento talhao {TalhaoId}", dto.TalhaoId);
    }
}
