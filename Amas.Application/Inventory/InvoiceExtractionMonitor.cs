namespace Amas.Application.Inventory;

public interface IInvoiceExtractionMonitor
{
    InvoiceExtractorStatusDto GetStatus();
    void Configure(OpenAiInvoiceOptions options);
    void RecordSuccess(string provider, string model);
    void RecordFailure(string provider, string model, string errorCode, string message);
}

public sealed class InvoiceExtractionMonitor : IInvoiceExtractionMonitor
{
    private readonly object syncRoot = new();
    private InvoiceExtractorStatusDto status = new(
        Provider: "openai",
        Model: "gpt-4.1-mini",
        Enabled: false,
        Configured: false,
        Available: false,
        Status: "Disabled",
        Message: "La extraccion con OpenAI esta deshabilitada.",
        LastErrorCode: null,
        LastCheckedAt: null);

    public InvoiceExtractorStatusDto GetStatus()
    {
        lock (syncRoot)
        {
            return status;
        }
    }

    public void Configure(OpenAiInvoiceOptions options)
    {
        lock (syncRoot)
        {
            var configured = !string.IsNullOrWhiteSpace(options.ApiKey);
            var enabled = options.Enabled;
            status = status with
            {
                Provider = "openai",
                Model = string.IsNullOrWhiteSpace(options.Model) ? "gpt-4.1-mini" : options.Model.Trim(),
                Enabled = enabled,
                Configured = configured,
                Available = enabled && configured && status.LastErrorCode is not ("insufficient_quota" or "rate_limit_exceeded"),
                Status = !enabled ? "Disabled" : configured ? status.Status == "Disabled" || status.Status == "NotConfigured" ? "Ready" : status.Status : "NotConfigured",
                Message = !enabled
                    ? "La extraccion con OpenAI esta deshabilitada."
                    : configured
                        ? status.Message == "La extraccion con OpenAI esta deshabilitada." ? "OpenAI esta configurado. La disponibilidad real se valida al procesar una factura." : status.Message
                        : "Falta configurar OpenAI__ApiKey para leer facturas automaticamente."
            };
        }
    }

    public void RecordSuccess(string provider, string model)
    {
        lock (syncRoot)
        {
            status = status with
            {
                Provider = provider,
                Model = model,
                Enabled = true,
                Configured = true,
                Available = true,
                Status = "Available",
                Message = "Ultima extraccion completada correctamente.",
                LastErrorCode = null,
                LastCheckedAt = DateTimeOffset.UtcNow
            };
        }
    }

    public void RecordFailure(string provider, string model, string errorCode, string message)
    {
        lock (syncRoot)
        {
            var quotaError = errorCode is "insufficient_quota" or "rate_limit_exceeded" or "quota_exceeded";
            status = status with
            {
                Provider = provider,
                Model = model,
                Enabled = true,
                Configured = true,
                Available = !quotaError,
                Status = quotaError ? "QuotaUnavailable" : "Error",
                Message = quotaError
                    ? "OpenAI no tiene cuota disponible o se alcanzo el limite de uso. La factura queda para revision manual."
                    : message,
                LastErrorCode = errorCode,
                LastCheckedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
