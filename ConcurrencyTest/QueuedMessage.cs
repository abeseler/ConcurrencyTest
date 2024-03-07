namespace ConcurrencyApp;

public sealed record QueuedMessage
{
    public int Id { get; init; }
    public string? Message { get; init; }
    public DateTime CreatedOn { get; init; }
    public DateTime InvisibleUntil { get; init; }
}
