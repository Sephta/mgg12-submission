using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickupManager : SingletonMonoBehavior<AbilityPickupManager>
{
  [SerializeField, Range(0f, 5f)] private float _pickupCooldown = 1f;
  [SerializeField, ReadOnly] private List<AbilityPickup> _pickups = new();

  // [Header("Debug")]
  // [SerializeField, ReadOnly] private bool _isWaiting = false;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  // private void Awake() {}

  private void Start()
  {
    if (Instance == null) return;

    AbilityPickup[] pickups = FindObjectsByType<AbilityPickup>(FindObjectsSortMode.None);

    _pickups = new(pickups);

    // _isWaiting = false;
  }

  // private void OnEnable() {}
  // private void OnDisable() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public void StartAbilityPickupCooldown(GameObject pickupGoingOnCooldown)
  {
    // Don't start another coroutine if we're already running one
    // if (_isWaiting) return;

    StartCoroutine(Wait(_pickupCooldown, pickupGoingOnCooldown));
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private IEnumerator Wait(float duration, GameObject objectToTempDisable)
  {
    // _isWaiting = true;
    objectToTempDisable.SetActive(false);
    yield return new WaitForSecondsRealtime(duration);
    objectToTempDisable.SetActive(true);
    // _isWaiting = false;
  }
}
