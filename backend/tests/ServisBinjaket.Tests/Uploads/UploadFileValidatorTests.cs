using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ServisBinjaket.Api.Uploads;

namespace ServisBinjaket.Tests.Uploads;

public class UploadFileValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WithValidJpeg_NormalizesStorageExtension()
    {
        var file = CreateFile(
            "photo.JPEG",
            "image/jpeg",
            [0xFF, 0xD8, 0xFF, 0xE0, 0x01]);

        var result = await UploadFileValidator.ValidateAsync(
            file,
            UploadFileScope.RepairAttachment,
            CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.Upload!.StorageExtension.Should().Be(".jpg");
        result.Upload.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task ValidateAsync_WithSpoofedImageSignature_RejectsFile()
    {
        var file = CreateFile("invoice.jpg", "image/jpeg", "%PDF-1.7"u8.ToArray());

        var result = await UploadFileValidator.ValidateAsync(
            file,
            UploadFileScope.RepairAttachment,
            CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_FILE_SIGNATURE");
    }

    [Fact]
    public async Task ValidateAsync_WithMismatchedMimeAndExtension_RejectsFile()
    {
        var file = CreateFile(
            "photo.png",
            "image/jpeg",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var result = await UploadFileValidator.ValidateAsync(
            file,
            UploadFileScope.RepairAttachment,
            CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_FILE_TYPE");
    }

    [Fact]
    public async Task ValidateAsync_WithPdfInImagesOnlyScope_RejectsFile()
    {
        var file = CreateFile("manual.pdf", "application/pdf", "%PDF-1.7"u8.ToArray());

        var result = await UploadFileValidator.ValidateAsync(
            file,
            UploadFileScope.ImagesOnly,
            CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_FILE_TYPE");
    }

    private static FormFile CreateFile(string fileName, string contentType, byte[] contents)
    {
        var stream = new MemoryStream(contents);
        return new FormFile(stream, 0, contents.Length, "files", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
