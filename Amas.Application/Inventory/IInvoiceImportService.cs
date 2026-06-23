using Amas.Application.Common;

namespace Amas.Application.Inventory;

public interface IInvoiceImportService
{
    Task<Result<IReadOnlyList<InvoiceImportDto>>> ListAsync(CancellationToken cancellationToken);
    Task<Result<InvoiceImportDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<InvoiceImportDto>> UploadAsync(UploadInvoiceImportRequest request, CancellationToken cancellationToken);
    Task<Result<InvoiceImportLineDto>> AddLineAsync(Guid importId, CreateInvoiceImportLineRequest request, CancellationToken cancellationToken);
    Task<Result<InvoiceImportLineDto>> UpdateLineAsync(Guid importId, Guid lineId, UpdateInvoiceImportLineRequest request, CancellationToken cancellationToken);
    Task<Result<InvoiceImportDto>> ConfirmAsync(Guid importId, ConfirmInvoiceImportRequest request, CancellationToken cancellationToken);
    Task<Result<InvoiceImportDto>> CancelAsync(Guid importId, CancellationToken cancellationToken);
}
