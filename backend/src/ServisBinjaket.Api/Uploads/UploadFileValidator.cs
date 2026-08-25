using Microsoft.AspNetCore.Http;

namespace ServisBinjaket.Api.Uploads;

internal enum UploadFileScope
{
    ImagesOnly,
    RepairAttachment
}

internal sealed record ValidatedUpload(
    string OriginalFileName,
    string StorageExtension,
    string ContentType);

internal sealed record UploadValidationResult(
    ValidatedUpload? Upload,
    string? ErrorCode,
    string? ErrorMessage)
{
    public bool IsValid => Upload is not null;

    public static UploadValidationResult Valid(ValidatedUpload upload) => new(upload, null, null);

    public static UploadValidationResult Invalid(string code, string message) => new(null, code, message);
}

internal static class UploadFileValidator
{
    private const int HeaderLength = 12;

    private static readonly IReadOnlyDictionary<string, FileTypeDefinition> AllowedTypes =
        new Dictionary<string, FileTypeDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = new(".jpg", "image/jpeg", FileSignature.Jpeg, true),
            [".jpeg"] = new(".jpg", "image/jpeg", FileSignature.Jpeg, true),
            [".png"] = new(".png", "image/png", FileSignature.Png, true),
            [".webp"] = new(".webp", "image/webp", FileSignature.WebP, true),
            [".mp4"] = new(".mp4", "video/mp4", FileSignature.Mp4, false),
            [".pdf"] = new(".pdf", "application/pdf", FileSignature.Pdf, false)
        };

    public static async ValueTask<UploadValidationResult> ValidateAsync(
        IFormFile file,
        UploadFileScope scope,
        CancellationToken cancellationToken)
    {
        var originalFileName = Path.GetFileName(file.FileName).Trim();
        if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Length > 300)
        {
            return UploadValidationResult.Invalid(
                "INVALID_FILE_NAME",
                "The file name is empty or exceeds 300 characters.");
        }

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedTypes.TryGetValue(extension, out var definition))
        {
            return UploadValidationResult.Invalid(
                "INVALID_FILE_TYPE",
                "The file extension is not allowed.");
        }

        if (scope == UploadFileScope.ImagesOnly && !definition.IsImage)
        {
            return UploadValidationResult.Invalid(
                "INVALID_FILE_TYPE",
                "Only JPEG, PNG, and WebP images are allowed.");
        }

        var claimedContentType = file.ContentType.Split(';', 2)[0].Trim();
        if (!claimedContentType.Equals(definition.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            return UploadValidationResult.Invalid(
                "INVALID_FILE_TYPE",
                "The file extension and content type do not match.");
        }

        var header = new byte[HeaderLength];
        await using var stream = file.OpenReadStream();
        var bytesRead = 0;
        while (bytesRead < header.Length)
        {
            var read = await stream.ReadAsync(header.AsMemory(bytesRead), cancellationToken);
            if (read == 0)
            {
                break;
            }

            bytesRead += read;
        }

        if (!HasExpectedSignature(header.AsSpan(0, bytesRead), definition.Signature))
        {
            return UploadValidationResult.Invalid(
                "INVALID_FILE_SIGNATURE",
                "The file contents do not match the declared file type.");
        }

        return UploadValidationResult.Valid(new ValidatedUpload(
            originalFileName,
            definition.StorageExtension,
            definition.ContentType));
    }

    private static bool HasExpectedSignature(ReadOnlySpan<byte> header, FileSignature signature)
    {
        return signature switch
        {
            FileSignature.Jpeg => header.Length >= 3 &&
                                  header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            FileSignature.Png => header.Length >= 8 &&
                                 header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            FileSignature.WebP => header.Length >= 12 &&
                                  header[..4].SequenceEqual("RIFF"u8) &&
                                  header.Slice(8, 4).SequenceEqual("WEBP"u8),
            FileSignature.Mp4 => header.Length >= 12 && header.Slice(4, 4).SequenceEqual("ftyp"u8),
            FileSignature.Pdf => header.Length >= 5 && header[..5].SequenceEqual("%PDF-"u8),
            _ => false
        };
    }

    private sealed record FileTypeDefinition(
        string StorageExtension,
        string ContentType,
        FileSignature Signature,
        bool IsImage);

    private enum FileSignature
    {
        Jpeg,
        Png,
        WebP,
        Mp4,
        Pdf
    }
}
