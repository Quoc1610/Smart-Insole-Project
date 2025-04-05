using Kinesis.Components;
using UnityEditor;
using UnityEngine;

namespace Kinesis.UnityInterface {
    [CustomEditor(typeof(JointData))]
    public class JointDataEditor : Editor {
        public override void OnInspectorGUI() {
            this.serializedObject.Update();

            if (GUILayout.Button(
                new GUIContent(
                    "Scan for Muscle Segments",
                    "Rebuilds list of spanning muscle segments. Use if autoscan is disabled."
                )
            )) {
                JointData jointData = (JointData)this.target;
                jointData.ScanMuscles();
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