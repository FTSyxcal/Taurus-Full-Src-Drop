
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Valve.Newtonsoft.Json;
namespace Taurus;
public sealed class SignedFeaturePolicy
{
  public long Revision;
  public DateTime ExpiresUtc;
  public string[] Disabled;
  public static SignedFeaturePolicy Verify(
    string json,
    string publicKeyJson,
    DateTime now,
    long previousRevision)
  {
    SignedFeaturePolicy.Envelope envelope = (json != null && json.Length <= 65536 ? JsonConvert.DeserializeObject<SignedFeaturePolicy.Envelope>(json) : throw new FormatException("Policy is too large.")) ?? throw new FormatException("Missing policy envelope.");
    SignedFeaturePolicy.PublicKey publicKey = JsonConvert.DeserializeObject<SignedFeaturePolicy.PublicKey>(publicKeyJson) ?? throw new FormatException("Missing public key.");
    byte[] numArray = Convert.FromBase64String(envelope.Payload ?? "");
    using (RSA rsa = RSA.Create())
    {
      rsa.ImportParameters(new RSAParameters()
      {
        Modulus = Convert.FromBase64String(publicKey.Modulus ?? ""),
        Exponent = Convert.FromBase64String(publicKey.Exponent ?? "")
      });
      if (rsa.KeySize >= 2048 )
      {
        if (rsa.VerifyData(numArray, Convert.FromBase64String(envelope.Signature ?? ""), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
          goto label_13;
      }
      throw new FormatException("Policy signature is invalid.");
    }
label_13:
    SignedFeaturePolicy signedFeaturePolicy = JsonConvert.DeserializeObject<SignedFeaturePolicy>(Encoding.UTF8.GetString(numArray)) ?? throw new FormatException("Missing policy payload.");
    if (signedFeaturePolicy.Revision <= 0L || signedFeaturePolicy.Revision < previousRevision || signedFeaturePolicy.ExpiresUtc.Kind != DateTimeKind.Utc || signedFeaturePolicy.ExpiresUtc <= now || signedFeaturePolicy.ExpiresUtc > now.AddDays(7.0) || signedFeaturePolicy.Disabled == null || signedFeaturePolicy.Disabled.Length > MenuCatalog.Buttons.Length)
      throw new FormatException("Invalid, expired or superseded policy.");
    HashSet<string> stringSet = new HashSet<string>((IEqualityComparer<string>) StringComparer.Ordinal);
    foreach (string name in signedFeaturePolicy.Disabled)
    {
      int index = MenuCatalog.Find(name);
      if (index < 0 || !SignedFeaturePolicy.CanDisable(MenuCatalog.Buttons[index]) || !stringSet.Add(name))
        throw new FormatException("Invalid disabled feature: " + name);
    }
    return signedFeaturePolicy;
  }
  public static bool CanDisable(MenuEntry entry)
  {
    return entry != null && entry.Target < 0 && entry.Group != 3 && entry.Group != 14 && entry.Group != 22 && entry.Group != 9 && entry.Group != 23 && entry.Group != 10;
  }
  private sealed class Envelope
  {
    public string Payload { get; set; }
    public string Signature { get; set; }
  }
  private sealed class PublicKey
  {
    public string Modulus { get; set; }
    public string Exponent { get; set; }
  }
}
