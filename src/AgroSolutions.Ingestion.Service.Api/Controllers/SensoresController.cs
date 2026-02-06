using AgroSolutions.Ingestion.Service.Application.Dtos.Sensor;
using AgroSolutions.Ingestion.Service.Application.Interfaces;
using AgroSolutions.Ingestion.Service.Domain.Exceptions.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace AgroSolutions.Ingestion.Service.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SensoresController : MainController
{
    private readonly IIngestionAppService _ingestionAppService;

    public SensoresController(IIngestionAppService ingestionAppService)
    {
        _ingestionAppService = ingestionAppService;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Recebe dados de sensores de um talhão")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ReceberDados([FromBody] ReceberDadosSensorDto dto)
    {
        await _ingestionAppService.ReceberDadosSensorAsync(dto);
        return Accepted();
    }
}
