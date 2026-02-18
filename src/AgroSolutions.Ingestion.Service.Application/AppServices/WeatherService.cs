using AgroSolutions.Ingestion.Service.Application.Dtos.Weather;
using AgroSolutions.Ingestion.Service.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgroSolutions.Ingestion.Service.Application.AppServices;

/// <summary>
/// Implementação do serviço de previsão do tempo usando Open-Meteo API
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WeatherService> _logger;

    private static readonly List<Localizacao> LocalizacoesPredefinidas = new()
    {
        new Localizacao { Nome = "Apple - Cupertino (EUA)", Latitude = "37.33", Longitude = "-122.00" },
        new Localizacao { Nome = "Google - Mountain View (EUA)", Latitude = "37.42", Longitude = "-122.08" },
        new Localizacao { Nome = "Microsoft - Redmond (EUA)", Latitude = "47.63", Longitude = "-122.12" },
        new Localizacao { Nome = "Meta - Menlo Park (EUA)", Latitude = "37.48", Longitude = "-122.14" },
        new Localizacao { Nome = "SpaceX - Hawthorne (EUA)", Latitude = "33.92", Longitude = "-118.32" },
        new Localizacao { Nome = "Amazon - Seattle (EUA)", Latitude = "47.62", Longitude = "-122.33" },
        new Localizacao { Nome = "OpenAI - São Francisco (EUA)", Latitude = "37.77", Longitude = "-122.41" },
        new Localizacao { Nome = "MIT - Cambridge (EUA)", Latitude = "42.36", Longitude = "-71.09" },
        new Localizacao { Nome = "ARM - Cambridge (Reino Unido)", Latitude = "52.20", Longitude = "0.12" },
        new Localizacao { Nome = "Akihabara - Tóquio (Japão)", Latitude = "35.69", Longitude = "139.77" }
    };

    public WeatherService(IHttpClientFactory httpClientFactory, ILogger<WeatherService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<PrevisaoClimaDto> ObterPrevisaoAleatoriaAsync()
    {
        var random = new Random();
        var localizacao = LocalizacoesPredefinidas[random.Next(LocalizacoesPredefinidas.Count)];

        _logger.LogInformation("Consultando previsão do tempo para {NomeLocalizacao}", localizacao.Nome);

        // Construir URL da API Open-Meteo
        var url = $"https://api.open-meteo.com/v1/forecast" +
                  $"?latitude={localizacao.Latitude.ToString()}" +
                  $"&longitude={localizacao.Longitude.ToString()}" +
                  $"&current=temperature_2m,relative_humidity_2m,windspeed_10m" +
                  $"&timezone=America/Sao_Paulo";

        var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        // Deserializar resposta
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var weatherResponse = JsonSerializer.Deserialize<OpenWeatherResponseDto>(json, options);

        if (weatherResponse?.Current == null)
        {
            throw new Exception("Falha ao obter dados de previsão do tempo");
        }

        var previsao = new PrevisaoClimaDto
        {
            NomeLocalizacao = localizacao.Nome,
            Latitude = localizacao.Latitude,
            Longitude = localizacao.Longitude,
            Temperatura = weatherResponse.Current.Temperature_2m,
            UmidadeRelativa = weatherResponse.Current.Relative_humidity_2m,
            VelocidadeVento = weatherResponse.Current.Windspeed_10m,
            DataConsulta = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Previsão obtida: {Temperatura}°C, {Umidade}% umidade, {Vento} km/h vento",
            previsao.Temperatura, previsao.UmidadeRelativa, previsao.VelocidadeVento);

        return previsao;
    }

    private class Localizacao
    {
        public string Nome { get; set; } = string.Empty;
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
