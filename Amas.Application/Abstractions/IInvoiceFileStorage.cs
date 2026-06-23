namespace Amas.Application.Abstractions;

public interface IInvoiceFileStorage
{
    Task<StoredInvoiceFile> SaveAsync(InvoiceFileStorageRequest request, CancellationToken cancellationToken);
}

public sealed record InvoiceFileStorageRequest(
    string Container,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    Stream Content);

public sealed record StoredInvoiceFile(
    string Url,
    string StoragePath,
    string StorageProvider,
    string FileName,
    string ContentType,
    long SizeBytes);
