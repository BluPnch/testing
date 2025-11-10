using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Domain.Interfaces;
using Server.Controllers.Models;
using Server.Controllers.Converters;
using Server.Extensions;


namespace Server.Controllers;

[ApiController]
[Route("/api/v1/clients")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IAuthService _authService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService clientService,
        IAuthService authService,
        ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Получить клиентов с возможностью фильтрации.
    /// </summary>
    /// <param name="companyName">Название компании клиента (опционально).</param>
    /// <param name="phoneNumber">Телефон клиента (опционально).</param>
    /// <response code="200">Список клиентов успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Клиент не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClientDTO>),
        Description = "Список клиентов успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные параметры запроса.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Клиент не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetClients(
        [FromQuery] string? companyName = null,
        [FromQuery] string? phoneNumber = null)
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

            if (!string.IsNullOrEmpty(companyName))
            {
                var client = await _clientService.GetClientByCompanyNameAsync(companyName);
                if (client == null)
                {
                    return NotFound("Клиент не найден");
                }
                return Ok(new List<ClientDTO> { ClientConverter.ToDTO(client) });
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var client = await _clientService.GetClientByPhoneNumberAsync(phoneNumber);
                if (client == null)
                {
                    return NotFound("Клиент не найден");
                }
                return Ok(new List<ClientDTO> { ClientConverter.ToDTO(client) });
            }

            var clients = await _clientService.GetAllClientsAsync();
            return Ok(ClientConverter.ToDTO(clients));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetClients));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetClients));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Client not found in method {MethodName}", nameof(GetClients));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetClients));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
        
    /// <summary>
    /// Получить клиента по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <response code="200">Клиент успешно получен.</response>
    /// <response code="400">Некорректный идентификатор.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Клиент не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ClientDTO), Description = "Клиент успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный идентификатор.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Клиент не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetClientById(Guid id)
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
                return BadRequest("Некорректный идентификатор клиента");
            }

            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound("Клиент не найден");
            }

            return Ok(ClientConverter.ToDTO(client));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetClientById));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetClientById));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Client not found in method {MethodName}", nameof(GetClientById));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetClientById));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Получить растения клиента.
    /// </summary>
    /// <response code="200">Список растений успешно получен.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Клиент не найден</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("plants")]
    [Authorize(Roles = "Client")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlantDTO>), Description = "Список растений успешно получен.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Клиент не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetClientPlants()
    {
        try
        {
            // Получаем ID пользователя из токена с помощью extension метода
            var clientId = User.GetUserIdFromToken();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Не удалось идентифицировать пользователя");
            }

            var client = await _clientService.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return NotFound("Клиент не найден");
            }

            var plants = await _clientService.GetClientPlantsAsync(clientId);
            return Ok(PlantConverter.ToDTO(plants));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetClientPlants));
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetClientPlants));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Получить записи журнала клиента.
    /// </summary>
    /// <response code="200">Список записей успешно получен.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Клиент не найден</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("journal-records")]
    [Authorize(Roles = "Client")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<JournalRecordDTO>), Description = "Список записей успешно получен.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Клиент не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetClientJournalRecords()
    {
        try
        {
            // Получаем ID пользователя из токена с помощью extension метода
            var clientId = User.GetUserIdFromToken();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Не удалось идентифицировать пользователя");
            }

            var client = await _clientService.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return NotFound("Клиент не найден");
            }

            var records = await _clientService.GetClientJournalRecordsAsync(clientId);
            return Ok(JournalRecordConverter.ToDTO(records));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetClientJournalRecords));
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetClientJournalRecords));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    
    /// <summary>
    /// Изменить роль пользователя.
    /// </summary>
    /// <param name="clientId">Идентификатор пользователя.</param>
    /// <param name="requestDto">Данные для изменения роли.</param>
    /// <response code="200">Роль пользователя успешно изменена.</response>
    /// <response code="400">Некорректные данные.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPatch("{clientId}/role")]
    [Authorize(Roles = "Administrator")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(AuthUserDTO), Description = "Роль пользователя успешно изменена.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Пользователь не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> UpdateUserRole(Guid clientId, UpdateUserRoleRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var administratorId = User.GetUserIdFromToken();
            if (administratorId == Guid.Empty)
            {
                return Unauthorized("Не удалось идентифицировать пользователя");
            }

            var currentUser = await _authService.GetUserByIdAsync(clientId);
            if (currentUser == null)
            {
                return NotFound("Пользователь не найден");
            }

            var updatedUser = await _authService.UpdateUserRoleAsync(clientId, requestDto.NewRole, administratorId);
            return Ok(AuthUserConverter.ToDTO(updatedUser));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(UpdateUserRole));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(UpdateUserRole));
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(UpdateUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}