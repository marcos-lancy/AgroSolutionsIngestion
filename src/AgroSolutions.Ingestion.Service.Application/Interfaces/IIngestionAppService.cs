using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;

namespace AgroSolutions.Ingestion.Service.Application.Interfaces;

public interface IIngestionAppService
{
    Task ReceberDadosSensorAsync(ReceberDadosSensorDto dto);
}
