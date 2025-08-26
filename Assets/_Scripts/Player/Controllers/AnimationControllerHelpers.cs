using System;
using UnityEngine.Rendering;

namespace stal.Helpers.Animation
{
  // Define animation states here. These should be the names of each node 
  // in the Animator graph that the aseprite importer generates.
  public enum AnimationStates
  {
    IDLE,
    RUN,
    JUMP,
    FALLING,
    AIM,
    ENVIRON01,
    COMBAT01,
    COMBAT02,
    COMBAT03,
    TRANSITION01,
    SHOOTBASIC,
    SHOOTALT
  }

  // Dictionaries are not serializable in the inspector by default in Unity. To get around this
  // we use Unity's "SerializedDictionary" wrapper class. This class itself requires that we 
  // extend it in a new class before use.
  [Serializable]
  public class StringIntDictionary : SerializedDictionary<string, int> { }

  [Serializable]
  public class IntStringDictionary : SerializedDictionary<int, string> { }
}
