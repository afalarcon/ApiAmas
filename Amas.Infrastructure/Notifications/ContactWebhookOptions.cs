namespace Amas.Infrastructure.Notifications;

public sealed class ContactWebhookOptions
{
    public bool Enabled { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public int TimeoutSeconds { get; set; } = 8;
}
