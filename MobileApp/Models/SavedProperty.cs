namespace MobileApp.Models;

public class SavedProperty
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Property? Property { get; set; }
}