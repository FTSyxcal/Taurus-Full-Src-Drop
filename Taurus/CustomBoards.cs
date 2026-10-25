
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
internal sealed class CustomBoards : IDisposable
{
  private readonly MenuState state;
  private readonly Material _colorMat;
  private bool _applied;
  private bool scanBoardMeshes = true;
  private float nextFind;
  private readonly Dictionary<Renderer, Material[]> _boards = new Dictionary<Renderer, Material[]>();
  private readonly Dictionary<Renderer, Material[]> replacements = new Dictionary<Renderer, Material[]>();
  private readonly Dictionary<Renderer, MaterialPropertyBlock> originalBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
  private readonly Dictionary<string, Renderer> _scenePlanes = new Dictionary<string, Renderer>();
  private readonly List<CustomBoards.JoinScreen> _joinScreens = new List<CustomBoards.JoinScreen>();
  private readonly List<CustomBoards.SavedText> texts = new List<CustomBoards.SavedText>();
  private readonly MaterialPropertyBlock paint = new MaterialPropertyBlock();
  public CustomBoards(MenuState state, Material material)
  {
    this.state = state;
    this._colorMat = new Material(material);
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
    SceneManager.sceneUnloaded += new UnityAction<Scene>(this.OnSceneUnloaded);
    for (int index = 0; index < SceneManager.sceneCount; ++index)
      this.OnSceneLoaded(SceneManager.GetSceneAt(index), (LoadSceneMode) 1);
  }
  public void Tick()
  {
    float unscaledTime = Time.unscaledTime;
    if ((double) unscaledTime >= (double) this.nextFind)
    {
      this.nextFind = unscaledTime + 10f;
      this.FindBoards();
      this.FindJoinScreens();
      this.FindText();
    }
    Color color1 = MenuPalette.Background(this.state, unscaledTime);
    Color color2 = Color.Lerp(color1, MenuPalette.Left(this.state, unscaledTime), 0.25f);
    Color color3 = Color.Lerp(color1, MenuPalette.Right(this.state, unscaledTime), 0.25f);
    this._colorMat.SetFloat("_UseGradient", 1f);
    this._colorMat.SetColor("_LeftColor", color2);
    this._colorMat.SetColor("_RightColor", color3);
    float num1 = this.state.IsOn("Freeze gradient") ? 0.0f : (float) ((double) unscaledTime * 3.1415927410125732 * 2.0) / this.state.GradientPeriod;
    foreach (Renderer key in this._boards.Keys.Concat<Renderer>((IEnumerable<Renderer>) this._scenePlanes.Values))
    {
      if (!((Object) key == (Object) null))
      {
        if (((Object) key.sharedMaterial != (Object) this._colorMat))
        {
          Material[] materialArray;
          if (this.replacements.TryGetValue(key, out materialArray))
            key.sharedMaterials = materialArray;
          else
            key.sharedMaterial = this._colorMat;
        }
        Bounds bounds = key.bounds;
        Vector3 vector3 = ((((Component) key).transform.right * Mathf.Cos(num1)) + (((Component) key).transform.up * Mathf.Sin(num1)));
        float num2 = Mathf.Max(0.01f, Vector3.Dot(new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z)), (bounds).size));
        key.GetPropertyBlock(this.paint);
        this.paint.SetVector("_DesignOrigin", ((Vector4)(bounds).center));
        this.paint.SetVector("_DesignAxis", ((Vector4)(vector3 / num2)));
        key.SetPropertyBlock(this.paint);
      }
    }
    if (this._applied)
      return;
    foreach (CustomBoards.JoinScreen joinScreen in this._joinScreens)
    {
      if (((Object) joinScreen.Template != (Object) null))
        this.SetScreens(joinScreen.Template, this._colorMat);
    }
    PhotonNetworkController.Instance?.UpdateTriggerScreens();
    this._applied = true;
  }
  private void Pin(Renderer renderer)
  {
    if (((Object) renderer == (Object) null) || this._boards.ContainsKey(renderer))
      return;
    this._boards[renderer] = renderer.sharedMaterials;
    this.replacements[renderer] = Enumerable.Repeat<Material>(this._colorMat, Mathf.Max(1, this._boards[renderer].Length)).ToArray<Material>();
    MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
    renderer.GetPropertyBlock(materialPropertyBlock);
    this.originalBlocks[renderer] = materialPropertyBlock;
  }
  private void FindBoards()
  {
    GorillaComputer instance = GorillaComputer.instance;
    if (((Object) instance != (Object) null))
    {
      this.Pin((Renderer) instance.computerScreenRenderer);
      this.Pin((Renderer) instance.wallScreenRenderer);
    }
    Transform transform1 = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom")?.transform;
    if (((Object) transform1 != (Object) null))
    {
      this.Pin(((Component) transform1.Find("TreeRoomInteractables/UI/CodeOfConduct_Group/StaticUnlit/screen"))?.GetComponent<Renderer>());
      this.Pin(((Component) transform1.Find("TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen"))?.GetComponent<Renderer>());
    }
    foreach (GorillaScoreBoard gorillaScoreBoard in Object.FindObjectsByType<GorillaScoreBoard>((FindObjectsInactive) 1, (FindObjectsSortMode) 0))
    {
      Scene scene = ((Component) gorillaScoreBoard).gameObject.scene;
      string key = $"{(scene).handle.ToString()}/{((Object) gorillaScoreBoard).GetInstanceID().ToString()}";
      Renderer renderer;
      if (!((Object) gorillaScoreBoard.boardText == (Object) null) && (!this._scenePlanes.TryGetValue(key, out renderer) || !((Object) renderer != (Object) null)))
      {
        RectTransform rectTransform = ((TMP_Text) gorillaScoreBoard.boardText).rectTransform;
        MeshRenderer panel = BoardSurface.FindPanel(rectTransform, ((Component) gorillaScoreBoard).GetComponentsInChildren<MeshRenderer>(true));
        if (((Object) panel == (Object) null) && ((Object) ((Component) gorillaScoreBoard).transform.parent != (Object) null))
          panel = BoardSurface.FindPanel(rectTransform, ((Component) ((Component) gorillaScoreBoard).transform.parent).GetComponentsInChildren<MeshRenderer>(true));
        this._scenePlanes[key] = BoardSurface.CreateBackdrop(rectTransform, this._colorMat, panel);
      }
    }
    if (!this.scanBoardMeshes)
      return;
    this.scanBoardMeshes = false;
    foreach (Renderer renderer in Object.FindObjectsByType<Renderer>((FindObjectsInactive) 1, (FindObjectsSortMode) 0))
    {
      if (!((Object) ((Component) renderer).GetComponentInParent<TMP_Text>() != (Object) null) && !((Object) ((Component) renderer).GetComponentInParent<TextMesh>() != (Object) null))
      {
        Bounds bounds = renderer.bounds;
        Vector3 size = (bounds).size;
        if ((double) (size).sqrMagnitude <= 128.0)
        {
          for (Transform transform2 = ((Component) renderer).transform; ((Object) transform2 != (Object) null); transform2 = transform2.parent)
          {
            if (((Object) transform2).name.Replace("_", "").Replace(" ", "").IndexOf("wallmonitor", StringComparison.OrdinalIgnoreCase) >= 0)
            {
              this.Pin(renderer);
              break;
            }
          }
        }
      }
    }
  }
  private void FindJoinScreens()
  {
    foreach (JoinTriggerUI joinTriggerUi in Object.FindObjectsByType<JoinTriggerUI>((FindObjectsSortMode) 0))
    {
      JoinTriggerUITemplate template = AccessTools.Field(typeof (JoinTriggerUI), "template")?.GetValue((object) joinTriggerUi) as JoinTriggerUITemplate;
      if (!((Object) template == (Object) null) && !this._joinScreens.Any<CustomBoards.JoinScreen>((Func<CustomBoards.JoinScreen, bool>) (j => ((Object) j.Template == (Object) template))))
      {
        this._joinScreens.Add(new CustomBoards.JoinScreen()
        {
          Template = template,
          Original = new Material[8]
          {
            template.ScreenBG_AbandonPartyAndSoloJoin,
            template.ScreenBG_AlreadyInRoom,
            template.ScreenBG_ChangingGameModeSoloJoin,
            template.ScreenBG_Error,
            template.ScreenBG_InPrivateRoom,
            template.ScreenBG_LeaveRoomAndGroupJoin,
            template.ScreenBG_LeaveRoomAndSoloJoin,
            template.ScreenBG_NotConnectedSoloJoin
          }
        });
        this.SetScreens(template, this._colorMat);
        PhotonNetworkController.Instance?.UpdateTriggerScreens();
      }
    }
  }
  private void FindText()
  {
    this.SetText("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/motdHeadingText", "<b>Taurus Paid</b>");
    this.SetText("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/motdHeadingText/motdBodyText", "Thank you for using Taurus!\n\n\n\n<size=65%><color=#A5B0C1>Taurus 1.4.0</color></size>");
  }
  private void SetText(string path, string content)
  {
    TMP_Text text = GameObject.Find(path)?.GetComponent<TMP_Text>();
    if (((Object) text == (Object) null))
      return;
    if (!this.texts.Any<CustomBoards.SavedText>((Func<CustomBoards.SavedText, bool>) (t => ((Object) t.Target == (Object) text))))
    {
      Behaviour component = (Behaviour) ((Component) text).GetComponent<PlayFabTitleDataTextDisplay>();
      this.texts.Add(new CustomBoards.SavedText()
      {
        Target = text,
        Text = text.text,
        Color = ((Graphic) text).color,
        Alignment = text.alignment,
        RichText = text.richText,
        AutoSize = text.enableAutoSizing,
        FontSize = text.fontSize,
        Minimum = text.fontSizeMin,
        Maximum = text.fontSizeMax,
        Margin = text.margin,
        Wrapping = text.textWrappingMode,
        Overflow = text.overflowMode,
        Updater = component,
        UpdaterEnabled = ((Object) component != (Object) null) && component.enabled
      });
      if (((Object) component != (Object) null))
        component.enabled = false;
    }
    text.richText = true;
    text.alignment = (TextAlignmentOptions) 514;
    ((Graphic) text).color = Color.white;
    text.text = content;
    CustomBoards.SavedText savedText = this.texts.First<CustomBoards.SavedText>((Func<CustomBoards.SavedText, bool>) (t => ((Object) t.Target == (Object) text)));
    text.enableAutoSizing = true;
    text.fontSizeMax = savedText.AutoSize ? savedText.Maximum : savedText.FontSize;
    text.fontSizeMin = Mathf.Max(1f, text.fontSizeMax * 0.55f);
    text.textWrappingMode = (TextWrappingModes) 1;
    text.overflowMode = (TextOverflowModes) 1;
    Rect rect = text.rectTransform.rect;
    text.margin = new Vector4((rect).width * 0.025f, (rect).height * 0.025f, (rect).width * 0.025f, (rect).height * 0.025f);
  }
  public void Dispose()
  {
    SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
    SceneManager.sceneUnloaded -= new UnityAction<Scene>(this.OnSceneUnloaded);
    this.Revert();
    foreach (KeyValuePair<Renderer, MaterialPropertyBlock> originalBlock in this.originalBlocks)
    {
      if (((Object) originalBlock.Key != (Object) null))
        originalBlock.Key.SetPropertyBlock(originalBlock.Value);
    }
    foreach (CustomBoards.SavedText text in this.texts)
    {
      if (((Object) text.Target != (Object) null))
      {
        text.Target.text = text.Text;
        ((Graphic) text.Target).color = text.Color;
        text.Target.alignment = text.Alignment;
        text.Target.richText = text.RichText;
        text.Target.enableAutoSizing = text.AutoSize;
        text.Target.fontSize = text.FontSize;
        text.Target.fontSizeMin = text.Minimum;
        text.Target.fontSizeMax = text.Maximum;
        text.Target.margin = text.Margin;
        text.Target.textWrappingMode = text.Wrapping;
        text.Target.overflowMode = text.Overflow;
        if (((Object) text.Updater != (Object) null))
          text.Updater.enabled = text.UpdaterEnabled;
      }
    }
    foreach (string sceneName in this._scenePlanes.Keys.ToArray<string>())
      this.RemovePlane(sceneName);
    NativeUiSnapshot.Destroy((Object) this._colorMat);
  }
  private void SetScreens(JoinTriggerUITemplate t, Material m)
  {
    t.ScreenBG_AbandonPartyAndSoloJoin = m;
    t.ScreenBG_AlreadyInRoom = m;
    t.ScreenBG_ChangingGameModeSoloJoin = m;
    t.ScreenBG_Error = m;
    t.ScreenBG_InPrivateRoom = m;
    t.ScreenBG_LeaveRoomAndGroupJoin = m;
    t.ScreenBG_LeaveRoomAndSoloJoin = m;
    t.ScreenBG_NotConnectedSoloJoin = m;
  }
  private void RestoreScreens(CustomBoards.JoinScreen j)
  {
    JoinTriggerUITemplate template = j.Template;
    Material[] original = j.Original;
    template.ScreenBG_AbandonPartyAndSoloJoin = original[0];
    template.ScreenBG_AlreadyInRoom = original[1];
    template.ScreenBG_ChangingGameModeSoloJoin = original[2];
    template.ScreenBG_Error = original[3];
    template.ScreenBG_InPrivateRoom = original[4];
    template.ScreenBG_LeaveRoomAndGroupJoin = original[5];
    template.ScreenBG_LeaveRoomAndSoloJoin = original[6];
    template.ScreenBG_NotConnectedSoloJoin = original[7];
  }
  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    this.nextFind = 0.0f;
    this._applied = false;
    this.scanBoardMeshes = true;
  }
  private void OnSceneUnloaded(Scene scene)
  {
    foreach (string sceneName in this._scenePlanes.Keys.Where<string>((Func<string, bool>) (key => key.StartsWith((scene).handle.ToString() + "/", StringComparison.Ordinal))).ToArray<string>())
      this.RemovePlane(sceneName);
    foreach (Renderer key in this._boards.Keys.Where<Renderer>((Func<Renderer, bool>) (renderer => ((Object) renderer == (Object) null))).ToArray<Renderer>())
    {
      this._boards.Remove(key);
      this.replacements.Remove(key);
      this.originalBlocks.Remove(key);
    }
    this.texts.RemoveAll((Predicate<CustomBoards.SavedText>) (saved => ((Object) saved.Target == (Object) null)));
    this._joinScreens.RemoveAll((Predicate<CustomBoards.JoinScreen>) (screen => ((Object) screen.Template == (Object) null)));
    this.nextFind = 0.0f;
    this.scanBoardMeshes = true;
  }
  private void RemovePlane(string sceneName)
  {
    Renderer renderer;
    if (!this._scenePlanes.TryGetValue(sceneName, out renderer))
      return;
    if (((Object) renderer != (Object) null))
      NativeUiSnapshot.Destroy((Object) ((Component) renderer).gameObject);
    this._scenePlanes.Remove(sceneName);
  }
  private void Revert()
  {
    foreach (KeyValuePair<Renderer, Material[]> board in this._boards)
    {
      if (((Object) board.Key != (Object) null))
        board.Key.sharedMaterials = board.Value;
    }
    foreach (Renderer renderer in this._scenePlanes.Values)
    {
      if (((Object) renderer != (Object) null))
        ((Component) renderer).gameObject.SetActive(false);
    }
    foreach (CustomBoards.JoinScreen joinScreen in this._joinScreens)
    {
      if (((Object) joinScreen.Template != (Object) null))
        this.RestoreScreens(joinScreen);
    }
    if (this._joinScreens.Count > 0)
    {
      PhotonNetworkController instance = PhotonNetworkController.Instance;
      if (((Object) instance != (Object) null))
        instance.UpdateTriggerScreens();
    }
    this._applied = false;
  }
  private struct JoinScreen
  {
    public JoinTriggerUITemplate Template;
    public Material[] Original;
  }
  private sealed class SavedText
  {
    public TMP_Text Target;
    public string Text;
    public Color Color;
    public TextAlignmentOptions Alignment;
    public bool RichText;
    public bool AutoSize;
    public float FontSize;
    public float Minimum;
    public float Maximum;
    public Vector4 Margin;
    public TextWrappingModes Wrapping;
    public TextOverflowModes Overflow;
    public Behaviour Updater;
    public bool UpdaterEnabled;
  }
}
