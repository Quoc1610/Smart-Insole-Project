 using UnityEngine;
 using UnityEditor;

namespace Kinesis.UnityInterface {
    [CustomPropertyDrawer(typeof(HideInNormalInspectorAttribute))]
    class HideInNormalInspectorDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return 0.0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {}
    }
}