namespace Amas.Application.Inventory;

public sealed class OpenAiInvoiceOptions
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4.1-mini";
    public string ResponsesUrl { get; set; } = "https://api.openai.com/v1/responses";
}
