using System.Collections;
using UnityEngine;

public class Hitstop : MonoBehaviour
{
  [SerializeField] private IntIntEventChannelSO _eventThatTriggersHitstop;

  [SerializeField, Range(0f, 1f)] private float _hitStopTimeframe = 0.5f;

  [SerializeField, ReadOnly] private bool _isWaiting = false;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  // private void Awake() {}

  // private void Start() {}

  private void OnEnable()
  {
    if (_eventThatTriggersHitstop != null)
    {
      _eventThatTriggersHitstop.OnEventRaised += StartHitstop;
    }
  }

  private void OnDisable()
  {
    if (_eventThatTriggersHitstop != null)
    {
      _eventThatTriggersHitstop.OnEventRaised -= StartHitstop;
    }
  }

  // private void Update() {}
  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void StartHitstop(int objectID, int damageAmount)
  {
    // Don't start another coroutine if we're already running one
    if (_isWaiting) return;

    StartCoroutine(Wait(_hitStopTimeframe));
  }

  private IEnumerator Wait(float duration)
  {
    _isWaiting = true;
    Time.timeScale = 0.0f;
    float cachedFixedDeltaTime = Time.fixedDeltaTime;
    Time.fixedDeltaTime = 0.0f;

    yield return new WaitForSecondsRealtime(duration);

    Time.timeScale = 1.0f;
    Time.fixedDeltaTime = cachedFixedDeltaTime;
    _isWaiting = false;
  }
}
