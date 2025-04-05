using Kinesis.Components;
using UnityEditor;
using UnityEngine;

namespace Kinesis.UnityInterface {
    [CustomEditor(typeof(MuscleRenderGizmo))]
    public class MuscleRenderGizmoEditor : Editor {
        public override void OnInspectorGUI() {
            this.serializedObject.Update();

            if (GUILayout.Button(new GUIContent(
                "Scan for Muscles",
                "Refresh muscle list in case auto-scan fails to trigger."
            ))) {
                MuscleRenderGizmo muscleRenderGizmo = (MuscleRenderGizmo)this.target;
                muscleRenderGizmo.ScanMuscles();
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