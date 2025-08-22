using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerHUDController : MonoBehaviour
{
  [Header("UI Document")]
  [SerializeField] private UIDocument _playerHUDDocument;

  [Space(10f)]
  [Header("FPS Tracker Settings")]
  [SerializeField] private string _defaultTextContent = "FPS - ";
  [SerializeField] private bool _useTimeInterval = true;
  [SerializeField, Range(0f, 1f), ShowIf("_useTimeInterval")] private float _timeInterval = 0f;
  private float _currentTimeInterval = 0f;
  [SerializeField, ReadOnly] float _averageFps = 0f;

  private VisualElement _rootVisualElement;
  private Label _fpsLabel;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_playerHUDDocument == null) _playerHUDDocument = GetComponent<UIDocument>();
    if (_playerHUDDocument == null)
    {
      Debug.LogError(name + " does not have defined " + _playerHUDDocument.GetType().Name + ".  Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _rootVisualElement = _playerHUDDocument.rootVisualElement;
  }

  private void OnEnable()
  {
    _fpsLabel = _rootVisualElement.Q<Label>("fps-counter");
    _fpsLabel.AddToClassList("hide");
  }

  // private void Start() {}

  private void Update()
  {
    UpdateFPSText();
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void UpdateFPSText()
  {
    if (_fpsLabel == null) return;

    if (_fpsLabel.ClassListContains("hide"))
    {
      _fpsLabel.RemoveFromClassList("hide");
    }

    if (_useTimeInterval)
      UpdateTimeTracked();

    _averageFps = 1f / Time.unscaledDeltaTime;

    if (_currentTimeInterval == 0 && _useTimeInterval)
    {
      ResetTimeTracked();
      _fpsLabel.text = _defaultTextContent + GetReadableAverageFps() + GetReadableDeltaTime();
    }
    else if (!_useTimeInterval)
    {
      _fpsLabel.text = _defaultTextContent + GetReadableAverageFps() + GetReadableDeltaTime();
    }
  }

  private void UpdateTimeTracked() => _currentTimeInterval = Mathf.Clamp(_currentTimeInterval - Time.deltaTime, 0f, _timeInterval);
  private void ResetTimeTracked() => _currentTimeInterval = _timeInterval;
  private string GetReadableDeltaTime() => " (" + Mathf.Floor(Time.deltaTime * 1000f).ToString() + " ms)";
  private string GetReadableAverageFps() => (Mathf.Round(_averageFps * 100) / 100f).ToString();
}
