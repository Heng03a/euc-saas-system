using System.Text;
using EucSaaS.Application.Interfaces;
using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Security;
using EucSaaS.Web.ViewModels.Api.AuditLogs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/auditlogs")]
[Authorize(
    AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme,
    Policy = AppPolicies.AdminOnly)]
public class AuditLogsApiController : ControllerBase
{
    private const int MaximumPageSize = 100;
    private const int MaximumExportRows = 10_000;

    private static readonly HashSet<string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "ChangedAt",
            "ScreenCode",
            "RecordId",
            "ActionType",
            "FieldName",
            "ChangedBy"
        };

    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogsApiController(
        AppDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns tenant-isolated audit records with searching,
    /// filtering, sorting and paging.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedAuditLogResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedAuditLogResponse>> GetAuditLogs(
        [FromQuery] AuditLogQueryRequest request,
        CancellationToken cancellationToken)
    {
        var validationMessage = ValidateRequest(request);

        if (validationMessage is not null)
        {
            return BadRequest(new
            {
                message = validationMessage
            });
        }

        var tenantId = _currentUserService.TenantId;

        if (tenantId == Guid.Empty)
        {
            return Unauthorized(new
            {
                message =
                    "TenantId was not found for the authenticated user."
            });
        }

        var query = BuildFilteredQuery(
            tenantId,
            request);

        var totalCount = await query.CountAsync(
            cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)request.PageSize);

        query = ApplySorting(
            query,
            request.SortBy,
            request.SortDirection);

        var items = await query
            .Skip(
                (request.PageNumber - 1) *
                request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,
                TenantId = x.TenantId!.Value,
                ScreenCode = x.ScreenCode,
                RecordId = x.RecordId,
                ActionType = x.ActionType,
                FieldName = x.FieldName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                ChangedBy = x.ChangedBy,
                ChangedAt = x.ChangedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(new PagedAuditLogResponse
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        });
    }

    /// <summary>
    /// Returns one tenant-isolated audit record.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(AuditLogResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogResponse>> GetAuditLog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        if (tenantId == Guid.Empty)
        {
            return Unauthorized(new
            {
                message =
                    "TenantId was not found for the authenticated user."
            });
        }

        var auditLog = await _context.AuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.TenantId.HasValue &&
                x.TenantId.Value == tenantId)
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,
                TenantId = x.TenantId!.Value,
                ScreenCode = x.ScreenCode,
                RecordId = x.RecordId,
                ActionType = x.ActionType,
                FieldName = x.FieldName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                ChangedBy = x.ChangedBy,
                ChangedAt = x.ChangedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (auditLog is null)
        {
            return NotFound(new
            {
                message = "Audit log record was not found."
            });
        }

        return Ok(auditLog);
    }

    /// <summary>
    /// Exports tenant-isolated audit records as a CSV file.
    /// The same filters and sorting used by the list endpoint apply.
    /// </summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] AuditLogQueryRequest request,
        CancellationToken cancellationToken)
    {
        var validationMessage = ValidateExportRequest(request);

        if (validationMessage is not null)
        {
            return BadRequest(new
            {
                message = validationMessage
            });
        }

        var tenantId = _currentUserService.TenantId;

        if (tenantId == Guid.Empty)
        {
            return Unauthorized(new
            {
                message =
                    "TenantId was not found for the authenticated user."
            });
        }

        var query = BuildFilteredQuery(
            tenantId,
            request);

        query = ApplySorting(
            query,
            request.SortBy,
            request.SortDirection);

        var auditLogs = await query
            .Take(MaximumExportRows + 1)
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,
                TenantId = x.TenantId!.Value,
                ScreenCode = x.ScreenCode,
                RecordId = x.RecordId,
                ActionType = x.ActionType,
                FieldName = x.FieldName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                ChangedBy = x.ChangedBy,
                ChangedAt = x.ChangedAt
            })
            .ToListAsync(cancellationToken);

        if (auditLogs.Count > MaximumExportRows)
        {
            return BadRequest(new
            {
                message =
                    $"The export exceeds the maximum of " +
                    $"{MaximumExportRows:N0} records. " +
                    "Apply additional filters and try again."
            });
        }

        var csv = BuildCsv(auditLogs);
        var fileBytes = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(csv))
            .ToArray();

        var fileName =
            $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(
            fileBytes,
            "text/csv; charset=utf-8",
            fileName);
    }

    private IQueryable<AuditLog> BuildFilteredQuery(
        Guid tenantId,
        AuditLogQueryRequest request)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(x =>
                x.TenantId.HasValue &&
                x.TenantId.Value == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var pattern = $"%{search}%";

            query = query.Where(x =>
                EF.Functions.ILike(
                    x.ScreenCode,
                    pattern) ||
                EF.Functions.ILike(
                    x.RecordId,
                    pattern) ||
                EF.Functions.ILike(
                    x.ActionType,
                    pattern) ||
                EF.Functions.ILike(
                    x.FieldName,
                    pattern) ||
                EF.Functions.ILike(
                    x.ChangedBy,
                    pattern) ||
                (
                    x.OldValue != null &&
                    EF.Functions.ILike(
                        x.OldValue,
                        pattern)
                ) ||
                (
                    x.NewValue != null &&
                    EF.Functions.ILike(
                        x.NewValue,
                        pattern)
                ));
        }

        if (!string.IsNullOrWhiteSpace(request.ScreenCode))
        {
            var screenCode = request.ScreenCode.Trim();

            query = query.Where(x =>
                x.ScreenCode == screenCode);
        }

        if (!string.IsNullOrWhiteSpace(request.RecordId))
        {
            var recordId = request.RecordId.Trim();

            query = query.Where(x =>
                x.RecordId == recordId);
        }

        if (!string.IsNullOrWhiteSpace(request.ActionType))
        {
            var actionType =
                request.ActionType
                    .Trim()
                    .ToUpperInvariant();

            query = query.Where(x =>
                x.ActionType == actionType);
        }

        if (!string.IsNullOrWhiteSpace(request.FieldName))
        {
            var fieldName = request.FieldName.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(
                    x.FieldName,
                    fieldName));
        }

        if (!string.IsNullOrWhiteSpace(request.ChangedBy))
        {
            var changedBy = request.ChangedBy.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(
                    x.ChangedBy,
                    $"%{changedBy}%"));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x =>
                x.ChangedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x =>
                x.ChangedAt <= request.ToDate.Value);
        }

        return query;
    }

    private static IQueryable<AuditLog> ApplySorting(
        IQueryable<AuditLog> query,
        string sortBy,
        string sortDirection)
    {
        var descending = sortDirection.Equals(
            "desc",
            StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "screencode" => descending
                ? query
                    .OrderByDescending(x => x.ScreenCode)
                    .ThenByDescending(x => x.ChangedAt)
                : query
                    .OrderBy(x => x.ScreenCode)
                    .ThenBy(x => x.ChangedAt),

            "recordid" => descending
                ? query
                    .OrderByDescending(x => x.RecordId)
                    .ThenByDescending(x => x.ChangedAt)
                : query
                    .OrderBy(x => x.RecordId)
                    .ThenBy(x => x.ChangedAt),

            "actiontype" => descending
                ? query
                    .OrderByDescending(x => x.ActionType)
                    .ThenByDescending(x => x.ChangedAt)
                : query
                    .OrderBy(x => x.ActionType)
                    .ThenBy(x => x.ChangedAt),

            "fieldname" => descending
                ? query
                    .OrderByDescending(x => x.FieldName)
                    .ThenByDescending(x => x.ChangedAt)
                : query
                    .OrderBy(x => x.FieldName)
                    .ThenBy(x => x.ChangedAt),

            "changedby" => descending
                ? query
                    .OrderByDescending(x => x.ChangedBy)
                    .ThenByDescending(x => x.ChangedAt)
                : query
                    .OrderBy(x => x.ChangedBy)
                    .ThenBy(x => x.ChangedAt),

            _ => descending
                ? query
                    .OrderByDescending(x => x.ChangedAt)
                    .ThenByDescending(x => x.Id)
                : query
                    .OrderBy(x => x.ChangedAt)
                    .ThenBy(x => x.Id)
        };
    }

    private static string BuildCsv(
        IEnumerable<AuditLogResponse> auditLogs)
    {
        var csv = new StringBuilder();

        csv.AppendLine(
            "Id,TenantId,ScreenCode,RecordId," +
            "ActionType,FieldName,OldValue,NewValue," +
            "ChangedBy,ChangedAt");

        foreach (var auditLog in auditLogs)
        {
            csv.Append(EscapeCsv(auditLog.Id.ToString()));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.TenantId.ToString()));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.ScreenCode));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.RecordId));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.ActionType));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.FieldName));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.OldValue));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.NewValue));
            csv.Append(',');
            csv.Append(EscapeCsv(auditLog.ChangedBy));
            csv.Append(',');
            csv.Append(
                EscapeCsv(
                    auditLog.ChangedAt.ToUniversalTime()
                        .ToString("O")));

            csv.AppendLine();
        }

        return csv.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        var escapedValue = value.Replace(
            "\"",
            "\"\"");

        return $"\"{escapedValue}\"";
    }

    private static string? ValidateRequest(
        AuditLogQueryRequest request)
    {
        var commonValidation =
            ValidateCommonRequest(request);

        if (commonValidation is not null)
        {
            return commonValidation;
        }

        if (request.PageNumber < 1)
        {
            return "PageNumber must be at least 1.";
        }

        if (request.PageSize < 1)
        {
            return "PageSize must be at least 1.";
        }

        if (request.PageSize > MaximumPageSize)
        {
            return
                $"PageSize cannot exceed {MaximumPageSize}.";
        }

        return null;
    }

    private static string? ValidateExportRequest(
        AuditLogQueryRequest request)
    {
        return ValidateCommonRequest(request);
    }

    private static string? ValidateCommonRequest(
        AuditLogQueryRequest request)
    {
        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value >
            request.ToDate.Value)
        {
            return
                "FromDate cannot be later than ToDate.";
        }

        if (!string.IsNullOrWhiteSpace(
                request.ActionType))
        {
            var actionType =
                request.ActionType
                    .Trim()
                    .ToUpperInvariant();

            var validActionTypes = new[]
            {
                "CREATE",
                "UPDATE",
                "DELETE"
            };

            if (!validActionTypes.Contains(actionType))
            {
                return
                    "ActionType must be CREATE, UPDATE, or DELETE.";
            }
        }

        if (string.IsNullOrWhiteSpace(request.SortBy) ||
            !AllowedSortColumns.Contains(request.SortBy))
        {
            return
                "SortBy must be ChangedAt, ScreenCode, " +
                "RecordId, ActionType, FieldName, or ChangedBy.";
        }

        if (!request.SortDirection.Equals(
                "asc",
                StringComparison.OrdinalIgnoreCase) &&
            !request.SortDirection.Equals(
                "desc",
                StringComparison.OrdinalIgnoreCase))
        {
            return
                "SortDirection must be asc or desc.";
        }

        return null;
    }
}
