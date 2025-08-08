using UnityEditor;
using UnityEngine;

// Thank you to user @Lev-Lukomskyi on the unity forums => https://discussions.unity.com/t/how-to-make-a-readonly-property-in-inspector/75448/5

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return EditorGUI.GetPropertyHeight(property, label, true);
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    GUI.enabled = false;
    EditorGUI.PropertyField(position, property, label, true);
    GUI.enabled = true;
  }
}
