using HelpDesk.Application.Common.Utils;

namespace HelpDesk.Tests.Common;

public class FileSignatureValidatorTests
{
    [Theory]
    [InlineData(".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 })] // %PDF-1.4
    [InlineData(".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    [InlineData(".jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 })]
    [InlineData(".jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 })]
    [InlineData(".zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 })]
    [InlineData(".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 })] // OOXML is a zip container
    [InlineData(".xlsx", new byte[] { 0x50, 0x4B, 0x03, 0x04 })]
    public void IsContentValidForExtension_MatchingSignature_ReturnsTrue(string extension, byte[] header)
    {
        Assert.True(FileSignatureValidator.IsContentValidForExtension(extension, header));
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".zip")]
    public void IsContentValidForExtension_PlainTextHeader_ReturnsFalse(string extension)
    {
        var plainText = "just some ordinary text content"u8.ToArray();

        Assert.False(FileSignatureValidator.IsContentValidForExtension(extension, plainText));
    }

    [Fact]
    public void IsContentValidForExtension_UnrecognizedExtension_ReturnsTrue()
    {
        // .txt (and any other extension with no registered signature) has nothing meaningful to
        // verify against, so it intentionally passes through unchecked.
        var plainText = "hello world"u8.ToArray();

        Assert.True(FileSignatureValidator.IsContentValidForExtension(".txt", plainText));
    }

    [Fact]
    public void IsContentValidForExtension_HeaderShorterThanSignature_ReturnsFalse()
    {
        byte[] tooShort = [0x25, 0x50];

        Assert.False(FileSignatureValidator.IsContentValidForExtension(".pdf", tooShort));
    }

    [Fact]
    public void IsContentValidForExtension_EmptyHeader_ReturnsFalse()
    {
        Assert.False(FileSignatureValidator.IsContentValidForExtension(".pdf", ReadOnlySpan<byte>.Empty));
    }
}
