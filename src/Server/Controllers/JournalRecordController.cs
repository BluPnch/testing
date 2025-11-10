using Domain.Interfaces.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Domain.Exceptions;
using Server.Controllers.Models;
using Server.Controllers.Converters;

namespace Server.Controllers;

[ApiController]
[Route("/api/v1/journal-records")]
public class JournalRecordController : ControllerBase
{
    private readonly IJournalRecordService _journalRecordService;
    private readonly ILogger<JournalRecordController> _logger;

    public JournalRecordController(IJournalRecordService journalRecordService, ILogger<JournalRecordController> logger)
    {
        _journalRecordService = journalRecordService;
        _logger = logger;
    }

    /// <summary>
    /// Получить записи журнала с возможностью фильтрации.
    /// </summary>
    /// <param name="plantId">Идентификатор растения (опционально).</param>
    /// <param name="startDate">Начальная дата периода (опционально).</param>
    /// <param name="endDate">Конечная дата периода (опционально).</param>
    /// <response code="200">Список записей успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Записи не найдены.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<JournalRecordDTO>), Description = "Список записей успешно получен.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные параметры запроса.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Записи не найдены.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetJournalRecords(
        [FromQuery] Guid? plantId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
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

            if (plantId.HasValue)
            {
                var records = await _journalRecordService.GetJournalRecordsByPlantIdAsync(plantId.Value);
                if (records == null || !records.Any())
                {
                    return NotFound("Записи журнала для указанного растения не найдены");
                }
                return Ok(JournalRecordConverter.ToDTO(records));
            }

            if (startDate.HasValue || endDate.HasValue)
            {
                if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                {
                    return BadRequest("Начальная дата не может быть больше конечной даты");
                }

                var records = await _journalRecordService.GetJournalRecordsByDateRangeAsync(
                    startDate, endDate);
                
                if (records == null || !records.Any())
                {
                    return NotFound("Записи журнала за указанный период не найдены");
                }
                return Ok(JournalRecordConverter.ToDTO(records));
            }

            var allRecords = await _journalRecordService.GetAllJournalRecordsAsync();
            return Ok(JournalRecordConverter.ToDTO(allRecords));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetJournalRecords));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetJournalRecords));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Journal records not found in method {MethodName}", nameof(GetJournalRecords));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetJournalRecords));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
        
        /// <summary>
    /// Получить запись журнала по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор записи журнала.</param>
    /// <response code="200">Запись журнала успешно получена.</response>
    /// <response code="400">Некорректный идентификатор.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Запись журнала не найдена.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(JournalRecordDTO), Description = "Запись журнала успешно получена.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный идентификатор.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Запись журнала не найдена.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetJournalRecordById(Guid id)
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
                return BadRequest("Некорректный идентификатор записи журнала");
            }

            var record = await _journalRecordService.GetJournalRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound("Запись журнала не найдена");
            }

            return Ok(JournalRecordConverter.ToDTO(record));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Unauthorized access in method {MethodName}", nameof(GetJournalRecordById));
            return Unauthorized(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetJournalRecordById));
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Journal record not found in method {MethodName}", nameof(GetJournalRecordById));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(GetJournalRecordById));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    
    /// <summary>
    /// Создать новую запись в журнале.
    /// </summary>
    /// <param name="journalRecord">Данные записи.</param>
    /// <response code="201">Запись успешно создана.</response>
    /// <response code="400">Некорректные данные записи.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPost]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(JournalRecordDTO), Description = "Запись успешно создана.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные записи.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Create([FromBody] JournalRecordDTO journalRecordDto)
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

            var journalRecord = JournalRecordConverter.ToDomain(journalRecordDto);
            var createdRecord = await _journalRecordService.CreateJournalRecordAsync(journalRecord);
            return CreatedAtAction(nameof(GetJournalRecords), new { id = createdRecord.Id }, JournalRecordConverter.ToDTO(createdRecord));
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
    /// Удалить запись в журнале.
    /// </summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <response code="204">Запись успешно удалена.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Запись не найдена.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Запись успешно удалена.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Запись не найдена.")]
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

            await _journalRecordService.DeleteJournalRecordAsync(id);
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

    /// <summary>
    /// Обновить запись в журнале.
    /// </summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <param name="journalRecord">Данные записи.</param>
    /// <response code="204">Запись успешно обновлена.</response>
    /// <response code="400">Некорректные данные записи.</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Запись не найдена.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Employee")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Запись успешно обновлена.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные записи.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Недостаточно прав.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Запись не найдена.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] JournalRecordDTO journalRecordDto)
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

            if (id != journalRecordDto.Id)
            {
                return BadRequest("ID в URL не совпадает с ID в теле запроса");
            }

            var journalRecord = JournalRecordConverter.ToDomain(journalRecordDto);
            await _journalRecordService.UpdateJournalRecordAsync(journalRecord);
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
        catch (JournalRecordNotFoundException e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(Update));
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in method {MethodName}", nameof(Update));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

}