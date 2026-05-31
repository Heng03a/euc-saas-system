using EucSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/screens")]
public class ScreenMetadataController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScreenMetadataController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{screenCode}/metadata")]
    public async Task<IActionResult> GetMetadata(string screenCode)
    {
var screen = await _context.ScreenDefinitions
    .Include(x => x.Columns)
    .Include(x => x.FormFields)
    .FirstOrDefaultAsync(x =>
        EF.Functions.ILike(x.ScreenCode, screenCode)
        && x.IsActive);
        if (screen == null)
        {
            return NotFound(new
            {
                message = $"Screen metadata not found for: {screenCode}"
            });
        }

        var result = new
        {
            screen = new
            {
                screen.Id,
                screen.ScreenCode,
                screen.ScreenName,
                screen.EntityName,
                screen.RoutePath,
                screen.Description
            },

            columns = screen.Columns
                .Where(x => x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new
                {
                    x.FieldName,
                    x.DisplayLabel,
                    x.DataType,
                    x.DisplayOrder,
                    x.IsSortable,
                    x.IsSearchable
                }),

            formFields = screen.FormFields
                .Where(x => x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new
                {
                    x.FieldName,
                    x.DisplayLabel,
                    x.ControlType,
                    x.DataType,
                    x.DisplayOrder,
                    x.IsRequired,
                    x.IsReadOnly,
                    x.MaxLength,
                    x.Placeholder
                })
        };

        return Ok(result);
    }
}
