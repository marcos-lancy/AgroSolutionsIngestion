namespace AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;

public class ReceberDadosSensorDto
{
    public Guid TalhaoId { get; set; }
    public decimal UmidadeSolo { get; set; }
    public decimal Temperatura { get; set; }
    public decimal Precipitacao { get; set; }
}
