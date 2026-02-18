namespace AgroSolutions.Ingestion.Service.Application.Dtos.Weather;

/// <summary>
/// DTO para resposta da API Open-Meteo
/// </summary>
public class OpenWeatherResponseDto
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public CurrentWeatherDto Current { get; set; } = new();
}

public class CurrentWeatherDto
{
    public string Time { get; set; } = string.Empty;
    public float Temperature_2m { get; set; }
    public float Relative_humidity_2m { get; set; }
    public float Windspeed_10m { get; set; }
}

/// <summary>
/// DTO para dados de previsão do tempo a serem salvos no banco
/// </summary>
public class PrevisaoClimaDto
{
    public string NomeLocalizacao { get; set; } = string.Empty;
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public float Temperatura { get; set; }
    public float UmidadeRelativa { get; set; }
    public float VelocidadeVento { get; set; }
    public DateTime DataConsulta { get; set; }
}
