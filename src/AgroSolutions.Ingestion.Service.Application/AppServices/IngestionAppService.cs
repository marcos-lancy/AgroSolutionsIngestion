using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;
using AgroSolutions.Ingestion.Service.Application.Interfaces;
using AgroSolutions.Ingestion.Service.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Ingestion.Service.Application.AppServices;

public class IngestionAppService : IIngestionAppService
{
    private readonly IMongoDbService _mongoDbService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<IngestionAppService> _logger;

    public IngestionAppService(
        IMongoDbService mongoDbService,
        IPublishEndpoint publishEndpoint,
        ILogger<IngestionAppService> logger)
    {
        _mongoDbService = mongoDbService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task ReceberDadosSensorAsync(ReceberDadosSensorDto dto)
    {
        _logger.LogInformation("Recebendo dados de sensor para talhão {TalhaoId}", dto.TalhaoId);

        // Salvar no MongoDB
        await _mongoDbService.SalvarDadosSensorAsync(dto);

        // Publicar evento no RabbitMQ para processamento assíncrono
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
