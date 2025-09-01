using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerHUDController : MonoBehaviour
{
  [Header("UI Document")]
  [SerializeField] private UIDocument _playerHUDDocument;

  [Space(10f)]
  [Required("Must provide a HSMScratchpadSO asset.")]
  [SerializeField, Expandable] private HSMScratchpadSO _scratchpad;
  private PlayerMovementDataSO _playerMovementData;
  private PlayerAttributesDataSO _playerAttributesData;
  private PlayerAbilityDataSO _playerAbilityData;
  private PlayerEventDataSO _playerEventData;
  [SerializeField] private PlayerHealthSO _playerHealth;

  [Space(10f)]
  [Header("FPS Tracker Settings")]
  [SerializeField] private string _defaultTextContent = "FPS - ";
  [SerializeField] private bool _useTimeInterval = true;
  [SerializeField, Range(0f, 1f), ShowIf("_useTimeInterval")] private float _timeInterval = 0f;
  private float _currentTimeInterval = 0f;
  [SerializeField, ReadOnly] float _averageFps = 0f;

  [Space(10f)]
  [Header("Health Bar Settings")]
  [SerializeField] private int _baseWidth = 78;

  private VisualElement _rootVisualElement;
  private Label _fpsLabel;
  private Label _currentArmLabel;
  private VisualElement _healthBarForeground;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_playerHUDDocument == null) _playerHUDDocument = GetComponent<UIDocument>();
    if (_playerHUDDocument == null)
    {
      Debug.LogError(name + " does not have a UIDocument referenced in the inspector.  Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_scratchpad == null)
    {
      Debug.LogError(name + " does not have a HSMScratchpadSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerMovementData = _scratchpad.GetScratchpadData<PlayerMovementDataSO>();
    if (_playerMovementData == null)
    {
      Debug.LogError(name + " does not have a PlayerMovementDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAttributesData = _scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
    if (_playerAttributesData == null)
    {
      Debug.LogError(name + " does not have a PlayerAttributesDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAbilityData = _scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
    if (_playerAbilityData == null)
    {
      Debug.LogError(name + " does not have a PlayerAbilityDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_playerAbilityData.ArmData.Count <= 0)
    {
      Debug.LogError(name + " contains empty PlayerAbilityDataSO.ArmData. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerEventData = _scratchpad.GetScratchpadData<PlayerEventDataSO>();
    if (_playerEventData == null)
    {
      Debug.LogError(name + " does not have a PlayerEventDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _rootVisualElement = _playerHUDDocument.rootVisualElement;
  }

  private void OnEnable()
  {
    _fpsLabel = _rootVisualElement.Q<Label>("fps-counter");
    _fpsLabel.AddToClassList("hide");

    _currentArmLabel = _rootVisualElement.Q<Label>("current-arm");

    _playerEventData.PlayerArmFinishedCycling.OnEventRaised += UpdateCurrentArmLabel;

    _healthBarForeground = _rootVisualElement.Q<VisualElement>("health-bar-foreground");

    if (_healthBarForeground != null && _playerHealth != null)
    {
      _healthBarForeground.style.width = _baseWidth;
    }

    SetHealthBarWidthBasedOnPlayerHealth();
  }

  private void OnDisable()
  {
    _playerEventData.PlayerArmFinishedCycling.OnEventRaised -= UpdateCurrentArmLabel;
  }

  private void Start()
  {
    UpdateCurrentArmLabel();
  }

  private void Update()
  {
    UpdateFPSText();
    SetHealthBarWidthBasedOnPlayerHealth();
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

  private void UpdateCurrentArmLabel()
  {
    if (_currentArmLabel != null && _playerAbilityData.CurrentlyEquippedArm != null)
    {
      _currentArmLabel.text = _playerAbilityData.CurrentlyEquippedArm.ArmType.ToString();
    }
  }

  private void SetHealthBarWidthBasedOnPlayerHealth()
  {
    if (_healthBarForeground == null || _playerHealth == null) return;

    float healthTranslation = Mathf.InverseLerp(_playerHealth.MinHealth, _playerHealth.MaxHealth, _playerHealth.CurrentHealth);

    _healthBarForeground.style.width = Mathf.Lerp(0, _baseWidth, healthTranslation);
  }
}
