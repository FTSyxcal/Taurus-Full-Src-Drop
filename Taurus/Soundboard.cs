
using Photon.Voice.Unity;
using POpusCodec.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
namespace Taurus;
internal sealed class Soundboard : IDisposable
{
  private readonly MonoBehaviour owner;
  private readonly MenuState state;
  private readonly Action refresh;
  private readonly Action<string, bool> notify;
  private readonly AudioSource source;
  private readonly string root;
  private readonly List<Soundboard.Item> items = new List<Soundboard.Item>();
  private string directory;
  private UnityWebRequest loading;
  private int generation;
  private bool ownedClip;
  private bool disposed;
  private double endTime;
  private Recorder recorder;
  private AudioClip micClip;
  private AudioClip previousClip;
  private Recorder.InputSourceType previousSource;
  private SamplingRate previousRate;
  private bool previousLoop;
  private bool previousDetection;
  public Soundboard(
    MonoBehaviour owner,
    MenuState state,
    string dataDirectory,
    Action refresh,
    Action<string, bool> notify)
  {
    Soundboard soundboard = this;
    this.owner = owner;
    this.state = state;
    this.refresh = refresh;
    this.notify = notify;
    this.root = Path.GetFullPath(Path.Combine(dataDirectory, "sounds"));
    this.directory = this.root;
    Directory.CreateDirectory(this.root);
    this.source = ((Component) owner).gameObject.AddComponent<AudioSource>();
    this.source.playOnAwake = false;
    this.source.spatialBlend = 0.0f;
    this.source.ignoreListenerPause = true;
    state.RegisterHandler("Browse sounds", (Action<MenuEntry>) (entry =>
    {
      soundboard.ReadDirectory();
      state.BrowseSounds(true);
    }));
    state.RegisterHandler("Stop sound", (Action<MenuEntry>) (entry => soundboard.Stop()));
    state.RegisterHandler("Loop sounds", (Action<MenuEntry>) (entry => soundboard.ApplySettings()));
    state.RegisterHandler("Reload sounds", (Action<MenuEntry>) (entry => soundboard.ReadDirectory()));
    state.RegisterHandler("Open sound folder", (Action<MenuEntry>) (entry => Process.Start(new ProcessStartInfo(soundboard.root)
    {
      UseShellExecute = true
    })));
    this.ReadDirectory();
  }
  private static string Caption(string name)
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (char ch in name)
      stringBuilder.Append(ch < ' ' || ch > '~' ? '?' : ch);
    return stringBuilder.Length > 42 ? stringBuilder.ToString(0, 39) + "..." : stringBuilder.ToString();
  }
  private void ReadDirectory()
  {
    if (!Directory.Exists(this.directory))
      this.directory = this.root;
    this.items.Clear();
    this.items.Add(new Soundboard.Item()
    {
      Name = "Soundboard controls",
      Controls = true
    });
    if (!string.Equals(this.directory, this.root, StringComparison.OrdinalIgnoreCase))
      this.items.Add(new Soundboard.Item()
      {
        Name = "Parent folder",
        Folder = true,
        Path = Directory.GetParent(this.directory).FullName
      });
    string[] directories = Directory.GetDirectories(this.directory);
    string[] files = Directory.GetFiles(this.directory);
    Array.Sort<string>(directories, (IComparer<string>) StringComparer.OrdinalIgnoreCase);
    Array.Sort<string>(files, (IComparer<string>) StringComparer.OrdinalIgnoreCase);
    foreach (string path in directories)
    {
      if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) == (FileAttributes) 0)
        this.items.Add(new Soundboard.Item()
        {
          Name = "Folder: " + Soundboard.Caption(Path.GetFileName(path)),
          Folder = true,
          Path = path
        });
    }
    foreach (string path in files)
    {
      string lowerInvariant = Path.GetExtension(path).ToLowerInvariant();
      if ((lowerInvariant == ".wav" || lowerInvariant == ".mp3" || lowerInvariant == ".ogg") && (File.GetAttributes(path) & FileAttributes.ReparsePoint) == (FileAttributes) 0)
        this.items.Add(new Soundboard.Item()
        {
          Name = Soundboard.Caption(Path.GetFileName(path)),
          Path = path
        });
    }
    this.state.SetSounds(this.items.ConvertAll<string>((Converter<Soundboard.Item, string>) (item => item.Name)).ToArray());
  }
  public void Press(int row)
  {
    int index = this.state.SoundIndexAt(row);
    if (index < 0 || index >= this.items.Count || this.disposed)
      return;
    Soundboard.Item obj = this.items[index];
    if (obj.Controls)
      this.state.BrowseSounds(false);
    else if (obj.Folder)
    {
      string fullPath = Path.GetFullPath(obj.Path);
      if (!string.Equals(fullPath, this.root, StringComparison.OrdinalIgnoreCase) && !fullPath.StartsWith(this.root + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
        return;
      this.directory = fullPath;
      this.ReadDirectory();
      this.state.BrowseSounds(true);
    }
    else
    {
      this.Stop();
      this.owner.StartCoroutine(this.Load(obj, this.generation));
    }
  }
  private IEnumerator Load(Soundboard.Item item, int ticket)
  {
    if (!File.Exists(item.Path) || new FileInfo(item.Path).Length > 33554432L )
    {
      this.notify("Sound missing or over 32 MB", true);
    }
    else
    {
      int num;
      switch (Path.GetExtension(item.Path).ToLowerInvariant())
      {
        case ".wav":
          num = 20;
          break;
        case ".ogg":
          num = 14;
          break;
        default:
          num = 13;
          break;
      }
      UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(item.Path).AbsoluteUri, (AudioType) num);
      bool flag;
      try
      {
        this.loading = request;
        request.timeout = 20;
        this.notify("Loading " + item.Name, false);
        yield return (object) request.SendWebRequest();
        if (this.loading == request)
          this.loading = (UnityWebRequest) null;
        if (this.disposed || ticket != this.generation)
          flag = false;
        else if (request.result != (UnityWebRequest.Result) 1)
        {
          this.notify("Could not decode sound", true);
          flag = false;
        }
        else
        {
          AudioClip clip = (AudioClip) null;
          try
          {
            clip = DownloadHandlerAudioClip.GetContent(request);
            if (((Object) clip == (Object) null) || (double) clip.length <= 0.0 || (double) clip.length > 180.0 || (long) clip.samples * (long) clip.channels > 16777216L )
              throw new InvalidDataException("Sound must be under three minutes and 64 MB decoded.");
            this.Play(clip, item.Name, true);
          }
          catch (Exception ex)
          {
            if (((Object) clip != (Object) null) && ((Object) this.source.clip != (Object) clip))
              Object.Destroy((Object) clip);
            this.notify("Sound failed: " + ex.Message, true);
          }
          goto label_17;
        }
        goto label_15;
      }
      finally
      {
        ((IDisposable) request)?.Dispose();
      }
label_17:
      request = (UnityWebRequest) null;
      yield break;
label_15:
      yield break;
    }
  }
  private void Play(AudioClip clip, string name, bool owned)
  {
    this.source.clip = clip;
    this.ownedClip = owned;
    this.ApplySettings();
    this.source.Play();
    this.endTime = AudioSettings.dspTime + (double) clip.length + 0.25;
    this.state.PlayingSound = name;
    string str = "local";
    Recorder myRecorder = GorillaTagger.Instance?.myRecorder;
    if (((Object) NetworkSystem.Instance != (Object) null) && NetworkSystem.Instance.InRoom && (double) this.state.MicVolume > 0.0)
    {
      if (!((Object) myRecorder == (Object) null) && myRecorder.IsInitialized && myRecorder.IsRecording && myRecorder.TransmitEnabled)
      {
        if (myRecorder.SourceType == null)
        {
          try
          {
            float[] numArray = new float[clip.samples * clip.channels];
            if (!clip.GetData(numArray, 0))
              throw new InvalidDataException("Audio samples not readable.");
            for (int index = 0; index < numArray.Length; ++index)
              numArray[index] *= this.state.MicVolume;
            this.micClip = AudioClip.Create("Taurus voice sound", clip.samples, clip.channels, clip.frequency, false);
            if (!this.micClip.SetData(numArray, 0))
              throw new InvalidDataException("Could not prepare voice samples.");
            this.previousSource = myRecorder.SourceType;
            this.previousClip = myRecorder.AudioClip;
            this.previousLoop = myRecorder.LoopAudioClip;
            this.previousDetection = myRecorder.VoiceDetection;
            this.previousRate = myRecorder.SamplingRate;
            this.recorder = myRecorder;
            this.recorder.AudioClip = this.micClip;
            this.recorder.LoopAudioClip = this.state.IsOn("Loop sounds");
            this.recorder.VoiceDetection = false;
            this.recorder.SourceType = (Recorder.InputSourceType) 1;
            this.recorder.RestartRecording(true);
            str = "local + mic";
            goto label_13;
          }
          catch (Exception ex)
          {
            this.RestoreMic();
            str = "local, mic failed";
            Debug.LogWarning((object) ("Taurus soundboard: " + ex?.ToString()));
            goto label_13;
          }
        }
      }
      str = "local, game mic unavailable";
    }
label_13:
    this.refresh();
    this.notify($"Playing {name} ({str})", false);
  }
  public void ApplySettings()
  {
    this.source.volume = this.state.SoundVolume;
    this.source.loop = this.state.IsOn("Loop sounds");
    if (((Object) this.recorder != (Object) null) && ((Object) this.recorder.AudioClip == (Object) this.micClip))
      this.recorder.LoopAudioClip = this.source.loop;
    if (this.source.loop || !((Object) this.source.clip != (Object) null))
      return;
    this.endTime = AudioSettings.dspTime + (double) Math.Max(0.0f, this.source.clip.length - this.source.time) + 0.25;
  }
  public void Tick()
  {
    if (((Object) this.source.clip == (Object) null))
      return;
    if (((Object) this.micClip != (Object) null) && (((Object) this.recorder == (Object) null) || ((Object) NetworkSystem.Instance == (Object) null) || !NetworkSystem.Instance.InRoom || ((Object) GorillaTagger.Instance?.myRecorder != (Object) this.recorder)))
    {
      this.Stop();
      this.refresh();
    }
    else
    {
      if (this.source.loop || AudioSettings.dspTime < this.endTime)
        return;
      this.Stop();
      this.refresh();
    }
  }
  private void RestoreMic()
  {
    try
    {
      if (!((Object) this.recorder != (Object) null) || this.recorder.SourceType != (Recorder.InputSourceType) 1 && this.recorder.SourceType != this.previousSource || !((Object) this.recorder.AudioClip == (Object) this.micClip))
        return;
      this.recorder.SourceType = this.previousSource;
      this.recorder.AudioClip = this.previousClip;
      this.recorder.LoopAudioClip = this.previousLoop;
      this.recorder.VoiceDetection = this.previousDetection;
      this.recorder.SamplingRate = this.previousRate;
      this.recorder.RestartRecording(true);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("Taurus microphone restore failed: " + ex?.ToString()));
    }
    finally
    {
      this.recorder = (Recorder) null;
      if (((Object) this.micClip != (Object) null))
        Object.Destroy((Object) this.micClip);
      this.micClip = (AudioClip) null;
    }
  }
  public void Stop()
  {
    ++this.generation;
    this.loading?.Abort();
    this.loading = (UnityWebRequest) null;
    this.source.Stop();
    this.RestoreMic();
    if (this.ownedClip && ((Object) this.source.clip != (Object) null))
      Object.Destroy((Object) this.source.clip);
    this.source.clip = (AudioClip) null;
    this.ownedClip = false;
    this.state.PlayingSound = "";
  }
  public void Dispose()
  {
    if (this.disposed)
      return;
    this.disposed = true;
    this.Stop();
    Object.Destroy((Object) this.source);
  }
  private sealed class Item
  {
    public string Name;
    public string Path;
    public bool Folder;
    public bool Controls;
  }
}
