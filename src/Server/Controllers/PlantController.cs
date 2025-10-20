using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Server.Controllers;

[ApiController]
[Route("/api/v1/plants")]
public class PlantController : ControllerBase
{
    private readonly IPlantService _plantService;
    private readonly ILogger<PlantController> _logger;

    public PlantController(IPlantService plantService, ILogger<PlantController> logger)
    {
        _plantService = plantService;
        _logger = logger;
    }

    /// <summary>
    /// Получить растения с возможностью фильтрации.
    /// </summary>
    /// <param name="family">Семейство растения (опционально).</param>
    /// <param name="species">Вид растения (опционально).</param>
    /// <response code="200">Список растений успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Растения не найдены.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<Plant>), Description = "Список растений успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные параметры запроса.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Растения не найдены.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetPlants(
        [FromQuery] string? family = null,
        [FromQuery] string? species = null)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (!string.IsNullOrEmpty(family))
            {
                var plants = await _plantService.GetPlantsByFamilyAsync(family);
                if (plants == null || !plants.Any())
                {
                    return NotFound("Растения указанного семейства не найдены");
                }
                return Ok(plants);
            }

            if (!string.IsNullOrEmpty(species))
            {
                var plants = await _plantService.GetPlantsBySpeciesAsync(species);
                if (plants == null || !plants.Any())
                {
                    return NotFound("Растения указанного вида не найдены");
                }
                return Ok(plants);
            }

            var allPlants = await _plantService.GetAllPlantsAsync();
            return Ok(allPlants);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetPlants));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetPlants));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Plants not found in method {MethodName}", nameof(GetPlants));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetPlants));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
        
    /// <summary>
    /// Получить растение по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор растения.</param>
    /// <response code="200">Растение успешно получено.</response>
    /// <response code="400">Некорректный идентификатор.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Растение не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(Plant), Description = "Растение успешно получено.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный идентификатор.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Растение не найдено.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetPlantById(Guid id)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (id == Guid.Empty)
            {
                return BadRequest("Некорректный идентификатор растения");
            }

            var plant = await _plantService.GetPlantByIdAsync(id);
            if (plant == null)
            {
                return NotFound("Растение не найдено");
            }

            return Ok(plant);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetPlantById));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetPlantById));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Plant not found in method {MethodName}", nameof(GetPlantById));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetPlantById));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Создать новое растение.
    /// </summary>
    /// <param name="plant">Данные растения.</param>
    /// <param name="clientId">Идентификатор клиента.</param>
    /// <response code="201">Растение успешно создано.</response>
    /// <response code="400">Некорректные данные.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPost]
    [Authorize]
    [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(Plant), Description = "Растение успешно создано.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Create([FromBody] Plant plant, [FromQuery] Guid clientId)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (clientId == Guid.Empty)
            {
                return BadRequest("Client ID is required");
            }

            var createdPlant = await _plantService.CreatePlantAsync(plant, clientId);
            return CreatedAtAction(nameof(GetPlants), new { id = createdPlant.Id }, createdPlant);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(Create));
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(Create));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Обновить данные растения.
    /// </summary>
    /// <param name="id">Идентификатор растения.</param>
    /// <param name="plant">Обновленные данные растения.</param>
    /// <response code="204">Растение успешно обновлено.</response>
    /// <response code="400">Некорректные данные растения.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Растение не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Растение успешно обновлено.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные растения.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Растение не найдено.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Plant plant)
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

            if (id != plant.Id)
                return BadRequest("ID in URL does not match ID in request body");

            await _plantService.UpdatePlantAsync(plant);
            return NoContent();
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(Update));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(Update));
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(Update));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Удалить растение.
    /// </summary>
    /// <param name="id">Идентификатор растения.</param>
    /// <response code="204">Растение успешно удалено.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Растение не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Растение успешно удалено.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Растение не найдено.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Delete(Guid id)
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

            await _plantService.DeletePlantAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(Delete));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(Delete));
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(Delete));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

}