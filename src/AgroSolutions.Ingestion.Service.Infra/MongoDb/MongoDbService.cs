using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;
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

    public async Task SalvarDadosSensorAsync(ReceberDadosSensorDto dto)
    {
        var documento = new DadosSensorDocument
        {
            Id = Guid.NewGuid(),
            TalhaoId = dto.TalhaoId,
            UmidadeSolo = dto.UmidadeSolo,
            Temperatura = dto.Temperatura,
            Precipitacao = dto.Precipitacao,
            Timestamp = DateTime.UtcNow
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
}
