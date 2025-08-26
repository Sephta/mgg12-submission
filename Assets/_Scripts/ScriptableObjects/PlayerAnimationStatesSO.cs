using System;
using stal.Helpers.Animation;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerAnimationStates Asset", menuName = "Scriptable Objects/PlayerAnimationStatesSO")]
public class PlayerAnimationStatesSO : ScriptableObject
{
  // Marked read only because they should be visible in the inspector but only editable in code.
  // We will map an enumeration defined above to an existing state name hash from the animator.
  // This will make it so that we do not need to rely on the string name of the animation state
  // to play the animation. We can just use the int hash stored in this dictionary.
  [ReadOnly] public StringIntDictionary StateNameToHash = new();

  // Its probably useful to store an easy way to get the animator state name using the int hash.
  // This container is populated using the button defined bellow (accessible from the inspector)
  [ReadOnly] public IntStringDictionary StateHashToName = new();

  private void OnEnable()
  {
    // Build/ReBuild the dictionaries on awake.
    PopulateAnimationDictionary();
  }

  private void OnDisable()
  {
    // Build/ReBuild the dictionaries on awake.
    PopulateAnimationDictionary();
  }

  private void PopulateAnimationDictionary()
  {
    // Wipe them away
    StateNameToHash.Clear();
    StateHashToName.Clear();

    // Build/Re-Build them
    foreach (AnimationStates animationState in (AnimationStates[])Enum.GetValues(typeof(AnimationStates)))
    {
      int hashedStateName = Animator.StringToHash(animationState.ToString().ToLower());
      StateNameToHash.Add(animationState.ToString(), hashedStateName);
      StateHashToName.Add(hashedStateName, animationState.ToString().ToLower());
    }
  }

}
