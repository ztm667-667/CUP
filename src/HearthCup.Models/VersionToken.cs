namespace HearthCup.Models;

/// <summary>
/// Converts the eight-byte SQL rowversion between bytes and the base64 hidden field used by the edit and
/// delete forms. Encoding lives in one place so every form round-trips the same token.
/// </summary>
public static class VersionToken
{
    private const int RowVersionLength = 8;

    public static string Encode(byte[] version) => Convert.ToBase64String(version);

    public static bool TryParse(string? value, out byte[] version)
    {
        version = [];
        if (string.IsNullOrWhiteSpace(value)) return false;

        try
        {
            var bytes = Convert.FromBase64String(value);
            if (bytes.Length != RowVersionLength) return false;
            version = bytes;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
