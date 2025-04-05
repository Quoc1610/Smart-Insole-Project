using Kinesis.Components;
using UnityEditor;
using UnityEngine;

namespace Kinesis.UnityInterface {
    [CustomEditor(typeof(MuscleGroup))]
    public class MuscleGroupEditor : Editor {
        public override void OnInspectorGUI() {
            this.serializedObject.Update();

            if (GUILayout.Button("Refresh All Muscles")) {
                MuscleGroup muscleGroup = (MuscleGroup)this.target;
                muscleGroup.Refresh();
            }

            // Draw default property drawers.
            string[] excludedProperties = new string[] {
                "m_Script" // Hide built-in script field.
            };
            Editor.DrawPropertiesExcluding(this.serializedObject, excludedProperties);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}