using System.Security.Cryptography;
using System.Text;
using CustomerService.Core.Ports.Out;
using Microsoft.Extensions.Configuration;

namespace CustomerService.Adapters.Security.AesGcm;

public sealed class AesGcmCustomerDataProtector : ICustomerDataProtector
{
    private readonly byte[] _key;

    public AesGcmCustomerDataProtector(IConfiguration cfg)
    {
        var b64 =
            Environment.GetEnvironmentVariable("SECURITY__ENC_KEY_BASE64")
            ?? cfg["Security:EncKeyBase64"]
            ?? cfg["SECURITY__ENC_KEY_BASE64"];

        if (string.IsNullOrWhiteSpace(b64))
            throw new InvalidOperationException("Missing Security:EncKeyBase64 (SECURITY__ENC_KEY_BASE64).");

        _key = Convert.FromBase64String(b64);

        if (_key.Length != 32)
            throw new InvalidOperationException("Security:EncKeyBase64 must be 32 bytes (Base64 of 32 bytes).");
    }

    public string ProtectSensitive(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
            return plaintext;

        // nonce 12 bytes (recomendado)
        var nonce = RandomNumberGenerator.GetBytes(12);
        var pt = Encoding.UTF8.GetBytes(plaintext);
        var ct = new byte[pt.Length];
        var tag = new byte[16];

        using var aes = new System.Security.Cryptography.AesGcm(_key);
        aes.Encrypt(nonce, pt, ct, tag);

        // payload: nonce|tag|cipher -> Base64
        var payload = new byte[nonce.Length + tag.Length + ct.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(ct, 0, payload, nonce.Length + tag.Length, ct.Length);

        return "AESGCM:" + Convert.ToBase64String(payload);
    }

    public string UnprotectSensitive(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
            return protectedValue;

        if (!protectedValue.StartsWith("AESGCM:", StringComparison.Ordinal))
            return protectedValue; // legado/plaintext

        var payloadB64 = protectedValue.Substring("AESGCM:".Length);
        var payload = Convert.FromBase64String(payloadB64);

        if (payload.Length < 12 + 16)
            throw new CryptographicException("Invalid AESGCM payload.");

        var nonce = payload.AsSpan(0, 12).ToArray();
        var tag = payload.AsSpan(12, 16).ToArray();
        var ct = payload.AsSpan(28).ToArray();
        var pt = new byte[ct.Length];

        using var aes = new System.Security.Cryptography.AesGcm(_key);
        aes.Decrypt(nonce, ct, tag, pt);

        return Encoding.UTF8.GetString(pt);
    }
}
