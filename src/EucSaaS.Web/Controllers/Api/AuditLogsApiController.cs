using EucSaaS.Application.Interfaces;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Api.AuditLogs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EucSaaS.Web.Security;

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
    /// Returns tenant-isolated audit records using optional filters and paging.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedAuditLogResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedAuditLogResponse>> GetAuditLogs(
        [FromQuery] AuditLogQueryRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult is not null)
        {
            return BadRequest(new
            {
                message = validationResult
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

        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(x =>
                x.TenantId.HasValue &&
                x.TenantId.Value == tenantId);

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
                request.ActionType.Trim().ToUpperInvariant();

            query = query.Where(x =>
                x.ActionType == actionType);
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

        var totalCount =
            await query.CountAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)request.PageSize);

        var items = await query
            .OrderByDescending(x => x.ChangedAt)
            .ThenByDescending(x => x.Id)
            .Skip(
                (request.PageNumber - 1) *
                request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,

                // The query already excludes null TenantId values.
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

        var response = new PagedAuditLogResponse
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return Ok(response);
    }

    /// <summary>
    /// Returns one tenant-isolated audit record.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(AuditLogResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
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

    private static string? ValidateRequest(
        AuditLogQueryRequest request)
    {
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

        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value > request.ToDate.Value)
        {
            return "FromDate cannot be later than ToDate.";
        }

        if (!string.IsNullOrWhiteSpace(request.ActionType))
        {
            var actionType =
                request.ActionType.Trim().ToUpperInvariant();

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

        return null;
    }
}
