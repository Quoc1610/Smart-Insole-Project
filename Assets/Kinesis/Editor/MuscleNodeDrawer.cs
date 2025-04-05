using Kinesis.Core;
using UnityEditor;
using UnityEngine;

namespace Kinesis.UnityInterface {
    [CustomPropertyDrawer(typeof(MuscleNode))]
    public class MuscleNodeDrawer : PropertyDrawer {
        const int Indent = 10;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(
                new Rect(
                    position.x + Indent,
                    position.y,
                    position.width - Indent,
                    position.height
                ),
                property,
                label,
                true
            );

            EditorGUI.EndProperty();
        }
    }
}