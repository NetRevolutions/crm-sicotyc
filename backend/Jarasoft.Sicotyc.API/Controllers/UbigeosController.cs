using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetAllDepartamentos;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetDistritosByDepartamentoProvincia;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetProvinciasByDepartamento;
using Microsoft.AspNetCore.Mvc;

namespace Jarasoft.Sicotyc.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UbigeosController : ControllerBase
    {
        private readonly GetAllDepartamentosHandler _departamentosHandler;
        private readonly GetProvinciasByDepartamentoHandler _provinciasHandler;
        private readonly GetDistritosByDepartamentoProvinciaHandler _distritosHandler;

        public UbigeosController(
        GetAllDepartamentosHandler departamentosHandler,
        GetProvinciasByDepartamentoHandler provinciasHandler,
        GetDistritosByDepartamentoProvinciaHandler distritosHandler)
        {
            _departamentosHandler = departamentosHandler;
            _provinciasHandler = provinciasHandler;
            _distritosHandler = distritosHandler;
        }

        [HttpGet("departamentos")]
        public async Task<IActionResult> GetDepartamentos(
        CancellationToken cancellationToken)
        {
            var result = await _departamentosHandler.HandleAsync(
                new GetAllDepartamentosQuery(),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("departamentos/{departamento}/provincias")]
        public async Task<IActionResult> GetProvincias(
        string departamento,
        CancellationToken cancellationToken)
        {
            var result = await _provinciasHandler.HandleAsync(
                new GetProvinciasByDepartamentoQuery(departamento),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("departamentos/{departamento}/provincias/{provincia}/distritos")]
        public async Task<IActionResult> GetDistritos(
        string departamento,
        string provincia,
        CancellationToken cancellationToken)
        {
            var result = await _distritosHandler.HandleAsync(
                new GetDistritosByDepartamentoProvinciaQuery(
                    departamento,
                    provincia),
                cancellationToken);

            return Ok(result);
        }
    }
}
