using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Server.Controllers.Models;
using Server.Controllers.Converters;

namespace Server.Controllers;

[ApiController]
[Route("/api/v1/seeds")]
public class SeedController : ControllerBase
{
    private readonly ISeedService _seedService;
    private readonly ILogger<SeedController> _logger;

    public SeedController(ISeedService seedService, ILogger<SeedController> logger)
    {
        _seedService = seedService;
        _logger = logger;
    }

    /// <summary>
    /// Получить семена с возможностью фильтрации.
    /// </summary>
    /// <param name="maturity">Зрелость семени (опционально).</param>
    /// <param name="viability">Жизнеспособность семени (опционально).</param>
    /// <response code="200">Список семян успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Семена не найдены.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<SeedDTO>), Description = "Список семян успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные параметры запроса.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Семена не найдены.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetSeeds(
        [FromQuery] string? maturity = null,
        [FromQuery] string? viability = null)
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

            if (!string.IsNullOrEmpty(maturity))
            {
                var seeds = await _seedService.GetSeedsByMaturityAsync(maturity);
                if (seeds == null || !seeds.Any())
                {
                    return NotFound("Семена с указанной зрелостью не найдены");
                }
                return Ok(SeedConverter.ToDTO(seeds));
            }

            if (!string.IsNullOrEmpty(viability))
            {
                var seeds = await _seedService.GetSeedsByViabilityAsync(viability);
                if (seeds == null || !seeds.Any())
                {
                    return NotFound("Семена с указанной жизнеспособностью не найдены");
                }
                return Ok(SeedConverter.ToDTO(seeds));
            }

            var allSeeds = await _seedService.GetAllSeedsAsync();
            return Ok(SeedConverter.ToDTO(allSeeds));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetSeeds));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetSeeds));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Seeds not found in method {MethodName}", nameof(GetSeeds));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetSeeds));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
        
    /// <summary>
    /// Получить семя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор семени.</param>
    /// <response code="200">Семя успешно получено.</response>
    /// <response code="400">Некорректный идентификатор.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Семя не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(SeedDTO), Description = "Семя успешно получено.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный идентификатор.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Семя не найдено.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetSeedById(Guid id)
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
                return BadRequest("Некорректный идентификатор семени");
            }

            var seed = await _seedService.GetSeedByIdAsync(id);
            if (seed == null)
            {
                return NotFound("Семя не найдено");
            }

            return Ok(SeedConverter.ToDTO(seed));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetSeedById));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetSeedById));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Seed not found in method {MethodName}", nameof(GetSeedById));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetSeedById));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Создать новое семя.
    /// </summary>
    /// <param name="seed">Данные семени.</param>
    /// <response code="201">Семя успешно создано.</response>
    /// <response code="400">Некорректные данные семени.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPost]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(SeedDTO), Description = "Семя успешно создано.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные семени.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Create([FromBody] SeedDTO seedDto)
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

            var seed = SeedConverter.ToDomain(seedDto);
            var createdSeed = await _seedService.CreateSeedAsync(seed);
            return CreatedAtAction(nameof(GetSeeds), new { id = createdSeed.Id }, SeedConverter.ToDTO(createdSeed));
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
    /// Обновить данные семени.
    /// </summary>
    /// <param name="id">Идентификатор семени.</param>
    /// <param name="seed">Обновленные данные семени.</param>
    /// <response code="204">Семя успешно обновлено.</response>
    /// <response code="400">Некорректные данные семени.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Семя не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Семя успешно обновлено.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные семени.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Семя не найдено.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SeedDTO seedDto)
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

            if (id != seedDto.Id)
                return BadRequest("ID in URL does not match ID in request body");

            var seed = SeedConverter.ToDomain(seedDto);
            await _seedService.UpdateSeedAsync(seed);
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
    /// Удалить семя.
    /// </summary>
    /// <param name="id">Идентификатор семени.</param>
    /// <response code="204">Семя успешно удалено.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Семя не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Семя успешно удалено.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Семя не найдено.")]
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

            await _seedService.DeleteSeedAsync(id);
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