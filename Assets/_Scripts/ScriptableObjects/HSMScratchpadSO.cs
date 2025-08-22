using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New HSM Scratchpad Data", menuName = "Scriptable Objects/HSM/Scratchpad")]
public class HSMScratchpadSO : ScriptableObject
{
  [Expandable] public List<ScratchpadDataSO> ScratchpadData = new();

  /// <summary>
  /// Returns the specific ScratchpadDataSO of the specified type. Or null if none found.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T GetScratchpadData<T>() where T : ScratchpadDataSO
  {
    foreach (var data in ScratchpadData)
    {
      if (data is T cast)
      {
        return cast;
      }
    }

    return null;
  }
}
