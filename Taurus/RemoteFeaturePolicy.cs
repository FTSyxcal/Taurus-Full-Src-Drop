
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Taurus;
internal sealed class RemoteFeaturePolicy : IDisposable
{
  private readonly ConfigEntry<string> url;
  private readonly ConfigEntry<string> publicKey;
  private readonly ConfigEntry<string> keyFingerprint;
  private readonly ConfigEntry<long> highestRevision;
  private readonly MenuState state;
  private readonly Action<string> report;
  private readonly HttpClient http = new HttpClient((HttpMessageHandler) new HttpClientHandler()
  {
    AllowAutoRedirect = false
  })
  {
    Timeout = TimeSpan.FromSeconds(10.0)
  };
  private Task<string> pending;
  private string requestedKey;
  private string requestedUrl;
  private string lastError;
  private string previousKey;
  private SignedFeaturePolicy current;
  private DateTime nextPoll;
  private long revision;
  public RemoteFeaturePolicy(ConfigFile config, MenuState state, Action<string> report)
  {
    this.state = state;
    this.report = report;
    this.url = config.Bind<string>("Remote safety policy", "ManifestUrl", "", "Optional HTTPS signed disable-only policy. Empty means no requests. Configured policies can turn off features, never turn them on.");
    this.publicKey = config.Bind<string>("Remote safety policy", "PublicKey", "", "RSA public key JSON with base64 Modulus and Exponent. Empty means remote control is inactive.");
    this.keyFingerprint = config.Bind<string>("Remote safety policy", "KeyFingerprint", "", "Automatically recorded public key identity for rollback protection.");
    this.highestRevision = config.Bind<long>("Remote safety policy", "HighestRevision", 0L, "Automatically recorded highest verified revision for this key. Do not decrease.");
    this.revision = Math.Max(0L, this.highestRevision.Value);
  }
  public bool Tick()
  {
    bool flag = false;
    DateTime utcNow = DateTime.UtcNow;
    if (this.current == null && this.pending == null && (string.IsNullOrWhiteSpace(this.url.Value) || string.IsNullOrWhiteSpace(this.publicKey.Value)))
      return false;
    if (this.previousKey != this.publicKey.Value)
    {
      this.previousKey = this.publicKey.Value;
      string base64String;
      using (SHA256 shA256 = SHA256.Create())
        base64String = Convert.ToBase64String(shA256.ComputeHash(Encoding.UTF8.GetBytes(this.publicKey.Value.Trim())));
      if (base64String != this.keyFingerprint.Value)
      {
        this.keyFingerprint.Value = base64String;
        this.highestRevision.Value = this.revision = 0L;
        if (this.current != null)
        {
          this.current = (SignedFeaturePolicy) null;
          this.state.ApplyDisabledFeatures((IEnumerable<string>) Array.Empty<string>());
          flag = true;
        }
      }
    }
    if (this.current != null && (utcNow >= this.current.ExpiresUtc || string.IsNullOrWhiteSpace(this.url.Value) || string.IsNullOrWhiteSpace(this.publicKey.Value)))
    {
      this.current = (SignedFeaturePolicy) null;
      this.state.ApplyDisabledFeatures((IEnumerable<string>) Array.Empty<string>());
      flag = true;
      this.report("Remote policy expired or unconfigured. Previously disabled features stay off.");
    }
    if (this.pending != null)
    {
      if (this.pending.IsCompleted)
      {
        try
        {
          string result = this.pending.GetAwaiter().GetResult();
          if (this.requestedKey != this.publicKey.Value || this.requestedUrl != this.url.Value)
            throw new InvalidDataException("Configuration changed during download.");
          string requestedKey = this.requestedKey;
          DateTime now = utcNow;
          long revision = this.revision;
          SignedFeaturePolicy signedFeaturePolicy = SignedFeaturePolicy.Verify(result, requestedKey, now, revision);
          if (this.current == null || signedFeaturePolicy.Revision > this.revision)
          {
            this.current = signedFeaturePolicy;
            this.revision = signedFeaturePolicy.Revision;
            this.highestRevision.Value = this.revision;
            this.state.ApplyDisabledFeatures((IEnumerable<string>) signedFeaturePolicy.Disabled);
            flag = true;
            if (this.state.Notice.StartsWith("Cleanup failed:", StringComparison.Ordinal))
              this.report(this.state.Notice);
            this.report($"Taurus safety policy {this.revision.ToString()}: {signedFeaturePolicy.Disabled.Length.ToString()} features remotely disabled.");
          }
          this.lastError = (string) null;
        }
        catch (Exception ex)
        {
          if (this.lastError != ex.Message)
            this.report("Remote policy ignored: " + ex.Message);
          this.lastError = ex.Message;
        }
        this.pending = (Task<string>) null;
      }
    }
    if (this.pending == null && utcNow >= this.nextPoll && !string.IsNullOrWhiteSpace(this.url.Value) && !string.IsNullOrWhiteSpace(this.publicKey.Value))
    {
      this.nextPoll = utcNow.AddSeconds(60.0);
      Uri result;
      if (Uri.TryCreate(this.url.Value, UriKind.Absolute, out result) && result.Scheme == Uri.UriSchemeHttps && result.UserInfo.Length == 0)
      {
        this.requestedKey = this.publicKey.Value;
        this.requestedUrl = this.url.Value;
        this.pending = this.Fetch(result);
      }
      else if (this.lastError != "HTTPS required")
      {
        this.lastError = "HTTPS required";
        this.report("Remote policy ignored: HTTPS required.");
      }
    }
    return flag;
  }
  private async Task<string> Fetch(Uri uri)
  {
    string str;
    using (CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10.0)))
    {
      using (HttpResponseMessage response = await this.http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false))
      {
        response.EnsureSuccessStatusCode();
        long? contentLength = response.Content.Headers.ContentLength;
        long num = 65536 ;
        if (contentLength.GetValueOrDefault() > num & contentLength.HasValue)
          throw new InvalidDataException("Policy is too large.");
        using (Stream input = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
        {
          using (MemoryStream output = new MemoryStream())
          {
            byte[] buffer = new byte[4096 ];
            while (true)
            {
              int count;
              if ((count = await input.ReadAsync(buffer, 0, buffer.Length, timeout.Token).ConfigureAwait(false)) > 0)
              {
                if (output.Length + (long) count <= 65536L )
                  output.Write(buffer, 0, count);
                else
                  break;
              }
              else
                goto label_13;
            }
            throw new InvalidDataException("Policy is too large.");
label_13:
            str = Encoding.UTF8.GetString(output.ToArray());
          }
        }
      }
    }
    return str;
  }
  public void Dispose() => this.http.Dispose();
}
