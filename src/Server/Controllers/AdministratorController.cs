using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Controllers.Models;
using Server.Controllers.Converters;
using Swashbuckle.AspNetCore.Annotations;


namespace Server.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("/api/v1/administrators")]
public class AdministratorController : ControllerBase
{
    private readonly IAdministratorService _administratorService;
    private readonly ILogger<AdministratorController> _logger;

    public AdministratorController(
        IAdministratorService administratorService,
        ILogger<AdministratorController> logger)
    {
        _administratorService = administratorService;
        _logger = logger;
    }
    
    
    /// <summary>
    /// Создать нового администратора.
    /// </summary>
    /// <param name="requestDto">Данные для создания администратора.</param>
    /// <response code="201">Администратор успешно создан.</response>
    /// <response code="400">Некорректные данные администратора.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(AdministratorDTO), Description = "Администратор успешно создан.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные администратора.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Create([FromBody] CreateAdministratorRequestDto requestDto)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (!User.IsInRole("Administrator"))
            {
                return Forbid("Недостаточно прав для выполнения операции");
            }

            var createdAdministrator = await _administratorService.CreateAdministratorAsync(
                Guid.NewGuid(), requestDto.PhoneNumber, requestDto.Surname, requestDto.Name, 
                requestDto.Patronymic, requestDto.Username, requestDto.Password);
            return CreatedAtAction(nameof(GetAllAdministrators), new { id = createdAdministrator.Id }, AdministratorConverter.ToDTO(createdAdministrator));
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
    /// Получить администраторов с возможностью фильтрации.
    /// </summary>
    /// <param name="surname">Фамилия администратора (опционально).</param>
    /// <param name="name">Имя администратора (опционально).</param>
    /// <param name="patronymic">Отчество администратора (опционально).</param>
    /// <param name="phoneNumber">Номер телефона администратора (опционально).</param>
    /// <response code="200">Список администраторов успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Администратор не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdministratorDTO>), Description = "Список администраторов успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные параметры запроса.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Администратор не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetAllAdministrators(
        [FromQuery] string? surname = null,
        [FromQuery] string? name = null,
        [FromQuery] string? patronymic = null,
        [FromQuery] string? phoneNumber = null)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (!User.IsInRole("Administrator"))
            {
                return Forbid("Недостаточно прав для выполнения операции");
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var administrator = await _administratorService.GetAdministratorByPhoneNumberAsync(phoneNumber);
                if (administrator == null)
                {
                    return NotFound("Администратор не найден");
                }
                return Ok(new List<AdministratorDTO> { AdministratorConverter.ToDTO(administrator) });
            }

            if (!string.IsNullOrEmpty(surname) && !string.IsNullOrEmpty(name))
            {
                var administrator = await _administratorService.GetAdministratorByFullNameAsync(
                    surname, name, patronymic ?? string.Empty);
                if (administrator == null)
                {
                    return NotFound("Администратор не найден");
                }
                return Ok(new List<AdministratorDTO> { AdministratorConverter.ToDTO(administrator) });
            }

            var administrators = await _administratorService.GetAllAdministratorsAsync();
            return Ok(AdministratorConverter.ToDTO(administrators));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetAllAdministrators));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetAllAdministrators));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Administrator not found in method {MethodName}", nameof(GetAllAdministrators));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetAllAdministrators));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
        
    /// <summary>
    /// Получить администратора по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор администратора.</param>
    /// <response code="200">Администратор успешно получен.</response>
    /// <response code="400">Некорректный идентификатор.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Администратор не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(AdministratorDTO), Description = "Администратор успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный идентификатор.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Администратор не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetAdministratorById(Guid id)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            if (!User.IsInRole("Administrator"))
            {
                return Forbid("Недостаточно прав для выполнения операции");
            }

            if (id == Guid.Empty)
            {
                return BadRequest("Некорректный идентификатор администратора");
            }

            var administrator = await _administratorService.GetAdministratorByIdAsync(id);
            if (administrator == null)
            {
                return NotFound("Администратор не найден");
            }

            return Ok(AdministratorConverter.ToDTO(administrator));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetAdministratorById));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetAdministratorById));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Administrator not found in method {MethodName}", nameof(GetAdministratorById));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetAdministratorById));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}