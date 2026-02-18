namespace AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;

public class ReceberDadosSensorDto
{
    public Guid TalhaoId { get; set; }
    public decimal UmidadeSolo { get; set; }
    public decimal Temperatura { get; set; }

    /// <summary>
    /// 0 (sem chuva dia seco)
    /// 1-10 (chuva leve)
    /// 11-50 (chuva moderada)
    /// >50 (chuva forte)
    /// </summary>
    public decimal Precipitacao { get; set; }
}
