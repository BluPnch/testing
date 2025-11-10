using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Server.Controllers.Models;
using Server.Controllers.Converters;

namespace Server.Controllers;

[ApiController]
[Route("/api/v1/growth-stages")]
public class GrowthStageController : ControllerBase
{
    private readonly IGrowthStageService _growthStageService;
    private readonly ILogger<GrowthStageController> _logger;

    public GrowthStageController(IGrowthStageService growthStageService, ILogger<GrowthStageController> logger)
    {
        _growthStageService = growthStageService;
        _logger = logger;
    }

    /// <summary>
    /// Получить стадии роста с возможностью фильтрации.
    /// </summary>
    /// <param name="name">Название стадии роста (опционально).</param>
    /// <response code="200">Список стадий роста успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Стадия роста не найдена.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<GrowthStageDTO>), Description = "Список стадий роста успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные параметры запроса.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Стадия роста не найдена.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetGrowthStages(
        [FromQuery] string? name = null)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (!User.IsInRole("Administrator") && !User.IsInRole("Employee"))
            {
                return Forbid("Недостаточно прав для выполнения операции");
            }

            // Если указано название - ищем по имени
            if (!string.IsNullOrEmpty(name))
            {
                var growthStage = await _growthStageService.GetGrowthStageByNameAsync(name);
                if (growthStage == null)
                {
                    return NotFound("Стадия роста с указанным названием не найдена");
                }
                return Ok(new List<GrowthStageDTO> { GrowthStageConverter.ToDTO(growthStage) });
            }

            // Если никакие фильтры не указаны - возвращаем все стадии роста
            var growthStages = await _growthStageService.GetAllGrowthStagesAsync();
            return Ok(GrowthStageConverter.ToDTO(growthStages));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetGrowthStages));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetGrowthStages));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Growth stage not found in method {MethodName}", nameof(GetGrowthStages));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetGrowthStages));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
        
    /// <summary>
    /// Получить стадию роста по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор стадии роста.</param>
    /// <response code="200">Стадия роста успешно получена.</response>
    /// <response code="400">Некорректный идентификатор.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Стадия роста не найдена.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(GrowthStageDTO), Description = "Стадия роста успешно получена.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный идентификатор.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Стадия роста не найдена.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetGrowthStageById(Guid id)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (!User.IsInRole("Administrator") && !User.IsInRole("Employee"))
            {
                return Forbid("Недостаточно прав для выполнения операции");
            }

            if (id == Guid.Empty)
            {
                return BadRequest("Некорректный идентификатор стадии роста");
            }

            var growthStage = await _growthStageService.GetGrowthStageByIdAsync(id);
            if (growthStage == null)
            {
                return NotFound("Стадия роста не найдена");
            }

            return Ok(GrowthStageConverter.ToDTO(growthStage));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetGrowthStageById));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetGrowthStageById));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Growth stage not found in method {MethodName}", nameof(GetGrowthStageById));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetGrowthStageById));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}