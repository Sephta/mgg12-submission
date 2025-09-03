using System;
using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachinePositionComposer))]
public class CameraController : MonoBehaviour
{
  [Required("Must provide a HSMScratchpadSO asset.")]
  [SerializeField] private HSMScratchpadSO _scratchpad;
  [SerializeField, Expandable] private PlayerAbilityDataSO _playerAbilityData;
  [SerializeField, Expandable] private PlayerEventDataSO _playerEventData;
  [SerializeField, Expandable] private PlayerAttributesDataSO _playerAttributesData;

  [Space(10)]

  [Header("Camera Settings")]

  [SerializeField] private CinemachinePositionComposer _postionComposer;

  [SerializeField, OnValueChanged(nameof(UpdatePositionComposer))]
  private Vector3 _baseTargetOffset = Vector3.zero;

  [SerializeField]
  private Vector2 _targetOffsetWhileMoving = Vector2.zero;

  [SerializeField]
  private Vector2 _targetOffsetWhileIdle = Vector2.zero;

  [SerializeField, Range(0f, 1f)]
  private float _transitionRate = 10f;

  [Header("Debug")]

  [SerializeField, ReadOnly]
  private float _desiredTargetOffsetX = 0f;

  [SerializeField, ReadOnly]
  private float _currentTargetOffsetX = 0f;

  [SerializeField, ReadOnly]
  private float _previousTargetOffsetX = 0f;

  [SerializeField, ReadOnly]
  private float _currentTransitionTimer = 1f;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_scratchpad == null)
    {
      Debug.LogError(name + " does not have a HSMScratchpadSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAbilityData = _scratchpad.GetScratchpadData<PlayerAbilityDataSO>();
    if (_playerAbilityData == null)
    {
      Debug.LogError(name + " does not have a PlayerAbilityDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }


    _playerEventData = _scratchpad.GetScratchpadData<PlayerEventDataSO>();
    if (_playerEventData == null)
    {
      Debug.LogError(name + " does not have a PlayerEventDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    _playerAttributesData = _scratchpad.GetScratchpadData<PlayerAttributesDataSO>();
    if (_playerAttributesData == null)
    {
      Debug.LogError(name + " does not have a PlayerAttributesDataSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (_postionComposer == null) GetComponent<CinemachinePositionComposer>();
  }

  // private void Start() {}
  // private void OnEnable() {}
  // private void OnDisable() {}

  private void Update()
  {
    _currentTransitionTimer = Mathf.Clamp(_currentTransitionTimer + (_transitionRate * Time.deltaTime), 0f, 1f);

    if (_playerAttributesData.IsAttacking
      || _playerAttributesData.IsNeedling
      || _playerAttributesData.IsLatchedOntoWall
      || (_playerAttributesData.IsTakingAim && _playerAbilityData.CurrentlyEquippedArmType == NeroArmType.Neutral))
    {
      _desiredTargetOffsetX = _baseTargetOffset.x;
    }
    else
    {
      float direction = 0f;

      if (_playerAttributesData.PlayerMoveDirection.x > 0) direction = 1f;
      else if (_playerAttributesData.PlayerMoveDirection.x < 0) direction = -1f;

      _desiredTargetOffsetX = _baseTargetOffset.x + (_targetOffsetWhileMoving.x * direction);
    }

    _currentTargetOffsetX = Mathf.Lerp(
      _currentTargetOffsetX,
      _desiredTargetOffsetX,
      _currentTransitionTimer
    );

    _postionComposer.TargetOffset = new(
      _currentTargetOffsetX,
      _baseTargetOffset.y,
      _baseTargetOffset.z
    );

    if (_desiredTargetOffsetX != _previousTargetOffsetX)
    {
      _previousTargetOffsetX = _desiredTargetOffsetX;
      _currentTransitionTimer = 0f;
    }
  }

  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
  private void UpdatePositionComposer()
  {
    if (_postionComposer == null) return;

    _postionComposer.TargetOffset = _baseTargetOffset;
  }
}
