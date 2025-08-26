using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet Data", menuName = "Scriptable Objects/BulletDataSO")]
public class BulletDataSO : ScriptableObject
{
  [field: SerializeField, Range(0f, 5f)]
  public float Speed { get; private set; }
}
