using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;
using AgroSolutions.Ingestion.Service.Application.Dtos.Weather;
using AgroSolutions.Ingestion.Service.Application.Interfaces;
using MongoDB.Driver;

namespace AgroSolutions.Ingestion.Service.Infra.MongoDb;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoCollection<DadosSensorDocument> _collection;

    public MongoDbService(IMongoDatabase database)
    {
        _collection = database.GetCollection<DadosSensorDocument>("sensores");
    }

    public async Task SalvarDadosSensorAsync(ReceberDadosSensorDto dto, PrevisaoClimaDto? previsaoClima = null)
    {
        var documento = new DadosSensorDocument
        {
            Id = Guid.NewGuid(),
            TalhaoId = dto.TalhaoId,
            UmidadeSolo = dto.UmidadeSolo,
            Temperatura = dto.Temperatura,
            Precipitacao = dto.Precipitacao,
            Timestamp = DateTime.UtcNow,
            // Dados de previsão do tempo
            PrevisaoClima = previsaoClima != null ? new PrevisaoClimaDocument
            {
                NomeLocalizacao = previsaoClima.NomeLocalizacao,
                Latitude = previsaoClima.Latitude,
                Longitude = previsaoClima.Longitude,
                Temperatura = previsaoClima.Temperatura,
                UmidadeRelativa = previsaoClima.UmidadeRelativa,
                VelocidadeVento = previsaoClima.VelocidadeVento,
                DataConsulta = previsaoClima.DataConsulta
            } : null
        };

        await _collection.InsertOneAsync(documento);
    }

    public async Task<List<ReceberDadosSensorDto>> ObterDadosPorTalhaoAsync(Guid talhaoId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var filterBuilder = Builders<DadosSensorDocument>.Filter;
        var filter = filterBuilder.Eq(x => x.TalhaoId, talhaoId);

        if (dataInicio.HasValue)
            filter &= filterBuilder.Gte(x => x.Timestamp, dataInicio.Value);

        if (dataFim.HasValue)
            filter &= filterBuilder.Lte(x => x.Timestamp, dataFim.Value);

        var documentos = await _collection
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();

        return documentos.Select(d => new ReceberDadosSensorDto
        {
            TalhaoId = d.TalhaoId,
            UmidadeSolo = d.UmidadeSolo,
            Temperatura = d.Temperatura,
            Precipitacao = d.Precipitacao
        }).ToList();
    }
}

public class DadosSensorDocument
{
    public Guid Id { get; set; }
    public Guid TalhaoId { get; set; }
    public decimal UmidadeSolo { get; set; }
    public decimal Temperatura { get; set; }
    public decimal Precipitacao { get; set; }
    public DateTime Timestamp { get; set; }

    public PrevisaoClimaDocument? PrevisaoClima { get; set; }
}

public class PrevisaoClimaDocument
{
    public string NomeLocalizacao { get; set; } = string.Empty;
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public float Temperatura { get; set; }
    public float UmidadeRelativa { get; set; }
    public float VelocidadeVento { get; set; }
    public DateTime DataConsulta { get; set; }
}
