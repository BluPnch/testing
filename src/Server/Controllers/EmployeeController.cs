using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Extensions;
using Server.Controllers.Models;
using Server.Controllers.Converters;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers;

[ApiController]
[Route("/api/v1/employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Получить сотрудников с возможностью фильтрации.
    /// </summary>
    /// <param name="phoneNumber">Номер телефона сотрудника (опционально).</param>
    /// <param name="task">Задача сотрудника (опционально).</param>
    /// <param name="plantDomain">Сфера растений сотрудника (опционально).</param>
    /// <response code="200">Список сотрудников успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Сотрудник не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmployeeDTO>), Description = "Список сотрудников успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные параметры запроса.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Сотрудник не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? phoneNumber = null,
        [FromQuery] string? task = null,
        [FromQuery] string? plantDomain = null)
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

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var employee = await _employeeService.GetEmployeeByPhoneNumberAsync(phoneNumber);
                if (employee == null)
                {
                    return NotFound("Сотрудник не найден");
                }
                return Ok(new List<EmployeeDTO> { EmployeeConverter.ToDTO(employee) });
            }

            if (!string.IsNullOrEmpty(task))
            {
                var employees = await _employeeService.GetEmployeesByTaskAsync(task);
                if (employees == null || !employees.Any())
                {
                    return NotFound("Сотрудники по указанной задаче не найдены");
                }
                return Ok(EmployeeConverter.ToDTO(employees));
            }

            if (!string.IsNullOrEmpty(plantDomain))
            {
                var employees = await _employeeService.GetEmployeesByPlantDomainAsync(plantDomain);
                if (employees == null || !employees.Any())
                {
                    return NotFound("Сотрудники по указанной сфере растений не найдены");
                }
                return Ok(EmployeeConverter.ToDTO(employees));
            }

            var allEmployees = await _employeeService.GetAllEmployeesAsync();
            return Ok(EmployeeConverter.ToDTO(allEmployees));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetEmployees));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetEmployees));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Employee not found in method {MethodName}", nameof(GetEmployees));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetEmployees));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
        
    /// <summary>
    /// Получить сотрудника по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор сотрудника.</param>
    /// <response code="200">Сотрудник успешно получен.</response>
    /// <response code="400">Некорректный идентификатор.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Сотрудник не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(EmployeeDTO), Description = "Сотрудник успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный идентификатор.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Сотрудник не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
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
                return BadRequest("Некорректный идентификатор сотрудника");
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Сотрудник не найден");
            }

            return Ok(EmployeeConverter.ToDTO(employee));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetEmployeeById));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetEmployeeById));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Employee not found in method {MethodName}", nameof(GetEmployeeById));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetEmployeeById));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // /// <summary>
    // /// Назначить сотрудника на растение.
    // /// </summary>
    // /// <param name="employeeId">Идентификатор сотрудника.</param>
    // /// <param name="plantId">Идентификатор растения.</param>
    // /// <response code="200">Сотрудник успешно назначен на растение.</response>
    // /// <response code="400">Некорректные данные.</response>
    // /// <response code="401">Пользователь не авторизован</response>
    // /// <response code="403">Недостаточно прав</response>
    // /// <response code="404">Сотрудник или растение не найдены.</response>
    // /// <response code="500">Ошибка на стороне сервера.</response>
    // [HttpPost("{employeeId}/plants/{plantId}")]
    // [Authorize(Roles = "Administrator")]
    // [SwaggerResponse(StatusCodes.Status200OK, "Сотрудник успешно назначен на растение.")]
    // [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные.")]
    // [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    // [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    // [SwaggerResponse(StatusCodes.Status404NotFound, "Сотрудник или растение не найдены.")]
    // [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    // public async Task<IActionResult> AssignToPlant(Guid employeeId, Guid plantId)
    // {
    //     try
    //     {
    //         if (!User.Identity.IsAuthenticated)
    //         {
    //             return Unauthorized("Пользователь не авторизован");
    //         }
    //
    //         if (!User.IsInRole("Administrator"))
    //         {
    //             return Forbid("Недостаточно прав для выполнения операции");
    //         }
    //
    //         await _employeeService.AssignEmployeeToPlantAsync(employeeId, plantId);
    //         return Ok();
    //     }
    //     catch (UnauthorizedAccessException e)
    //     {
    //         _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(AssignToPlant));
    //         return Unauthorized(e.Message);
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogError(e, "Error in method {MethodName}", nameof(AssignToPlant));
    //         return StatusCode(StatusCodes.Status500InternalServerError);
    //     }
    // }

    /// <summary>
    /// Получить растения сотрудника.
    /// </summary>
    /// <param name="employeeId">Идентификатор сотрудника.</param>
    /// <response code="200">Список растений успешно получен.</response>
    /// <response code="400">Некорректные данные.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Сотрудник не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("plants")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlantDTO>), Description = "Список растений успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Сотрудник не найден.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetPlants(Guid employeeId)
    {
        try
        {
            var currentUserId = User.GetUserIdFromToken();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized("Не удалось идентифицировать пользователя");
            }

            if (User.IsInRole("Employee") && currentUserId != employeeId)
            {
                return Forbid("Недостаточно прав для просмотра растений другого сотрудника");
            }

            var plants = await _employeeService.GetPlantsByEmployeeIdAsync(employeeId);
            return Ok(PlantConverter.ToDTO(plants));
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetPlants));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}