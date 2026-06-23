namespace Amas.Application.Products;

public sealed record ProductImageDto(
    Guid Id,
    Guid ProductId,
    string Url,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? AltText,
    int SortOrder,
    bool IsPrimary,
    string StorageProvider);

public sealed record UploadProductImageFile(
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    string? AltText);

public sealed record ReorderProductImageRequest(Guid ImageId, int SortOrder);
