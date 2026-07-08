namespace HelpDesk.Application.Common.Utils;

/// <summary>
/// Verifies a file's actual leading bytes match what its extension claims, so renaming
/// <c>malware.exe</c> to <c>malware.pdf</c> doesn't slip past the extension whitelist alone.
/// Extensions with no reliable magic number (e.g. <c>.txt</c>) are intentionally not checked —
/// there's nothing meaningful to verify them against.
/// </summary>
public static class FileSignatureValidator
{
    private static readonly Dictionary<string, byte[][]> SignaturesByExtension = new()
    {
        [".pdf"] = [[0x25, 0x50, 0x44, 0x46]], // %PDF
        [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        // .docx/.xlsx are OOXML, which is itself a zip archive — same signature as .zip.
        [".zip"] = [[0x50, 0x4B, 0x03, 0x04], [0x50, 0x4B, 0x05, 0x06], [0x50, 0x4B, 0x07, 0x08]],
        [".docx"] = [[0x50, 0x4B, 0x03, 0x04]],
        [".xlsx"] = [[0x50, 0x4B, 0x03, 0x04]],
    };

    public static bool IsContentValidForExtension(string extension, ReadOnlySpan<byte> header)
    {
        if (!SignaturesByExtension.TryGetValue(extension, out var candidates))
        {
            return true;
        }

        foreach (var signature in candidates)
        {
            if (header.Length >= signature.Length && header[..signature.Length].SequenceEqual(signature))
            {
                return true;
            }
        }

        return false;
    }
}
