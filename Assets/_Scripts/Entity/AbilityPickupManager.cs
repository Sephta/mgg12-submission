using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickupManager : SingletonMonoBehavior<AbilityPickupManager>
{
  [SerializeField, Range(0f, 5f)] private float _pickupCooldown = 1f;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  // private void Awake() {}
  // private void Start() {}
  // private void OnEnable() {}
  // private void OnDisable() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  public void StartAbilityPickupCooldown(GameObject pickupGoingOnCooldown)
  {
    StartCoroutine(Wait(_pickupCooldown, pickupGoingOnCooldown));
  }

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private IEnumerator Wait(float duration, GameObject objectToTempDisable)
  {
    objectToTempDisable.SetActive(false);
    yield return new WaitForSecondsRealtime(duration);
    objectToTempDisable.SetActive(true);
  }
}
