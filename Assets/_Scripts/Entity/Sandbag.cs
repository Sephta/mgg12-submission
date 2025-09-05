using UnityEngine;

public class Sandbag : MonoBehaviour
{
  [SerializeField] private IntIntIntEventChannelSO _damageEvent;

  private void Awake()
  {
    if (_damageEvent == null)
    {
      Debug.LogError(name + " does not have a IntIntEventChannelSO referenced in the inspector. Deactivating object to avoid null object errors.");
      gameObject.SetActive(false);
    }
  }

  // private void Start() {}

  private void OnEnable()
  {
    _damageEvent.OnEventRaised += DoDamageToEntity;
  }

  private void OnDisable()
  {
    _damageEvent.OnEventRaised -= DoDamageToEntity;
  }

  // private void Update() {}

  private void DoDamageToEntity(int objectID, int damageAmount, int knockbackForce)
  {
    if (objectID == gameObject.GetInstanceID())
    {
      Debug.Log("<ID: " + objectID + "> Attempting to do " + damageAmount + " to " + name);
    }
  }

}
