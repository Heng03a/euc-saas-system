namespace EucSaaS.Domain.Entities;

public class ScreenPermission
{
    public Guid Id { get; set; }

    public Guid ScreenDefinitionId { get; set; }
    public ScreenDefinition ScreenDefinition { get; set; } = null!;

    public string RoleName { get; set; } = string.Empty;

    public bool CanView { get; set; } = true;
    public bool CanAdd { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanExport { get; set; } = false;
}
