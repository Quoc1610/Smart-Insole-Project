using Kinesis.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kinesis.UnityInterface {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MuscleTendonUnit))]
    public class MuscleTendonUnitEditor : Editor {
        SerializedProperty autoBuildSegmentsProp;
        SerializedProperty muscleNodesProp;
        SerializedProperty muscleSegmentsProp;
        SerializedProperty parallelDampingOnProp;
        SerializedProperty parallelElasticityOnProp;
        ReorderableList muscleNodesList;
        bool unfoldMuscleNodes = true;
        MuscleTendonUnit muscle;

        float OnDrawGetHeight(int index) {
            SerializedProperty element = this.muscleNodesList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element);
        }

        void OnDrawMuscleNode(Rect rect, int index, bool isActive, bool isFocused) {
            SerializedProperty element = this.muscleNodesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element);
        }

        void OnEnable() {
            this.autoBuildSegmentsProp = this.serializedObject.FindProperty("autoBuildSegments");
            this.parallelDampingOnProp = this.serializedObject.FindProperty("parallelDampingOn");
            this.parallelElasticityOnProp = this.serializedObject.FindProperty("parallelElasticityOn");
            this.muscleNodesProp = this.serializedObject.FindProperty("muscleNodes");
            this.muscleSegmentsProp = this.serializedObject.FindProperty("muscleSegments");

            // Set up reorderable list.
            this.muscleNodesList = new ReorderableList(this.serializedObject, muscleNodesProp, true, false, true, true);
            this.muscleNodesList.headerHeight = 3;
            this.muscleNodesList.elementHeightCallback = this.OnDrawGetHeight;
            this.muscleNodesList.drawElementCallback = this.OnDrawMuscleNode;
        }

        public override void OnInspectorGUI() {
            this.serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Reminder: If auto-build is disabled, remember to refresh muscle segments after muscle node updates.",
                MessageType.Info
            );

            if (GUILayout.Button("Refresh Muscle Segments")) {
                foreach (MuscleTendonUnit target in this.targets) {
                    target.GenerateMuscleSegments();
                }
                // Update serialized property values from target data.
                muscleNodesProp.serializedObject.Update();
            }

            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

            // Draw default property drawers.
            string[] excludedProperties = new string[] {
                "autoBuildSegments",
                "joints",
                "muscleNodes",
                "muscleSegments",
                "parallelDampingOn",
                "parallelElasticityOn",
                "m_Script" // Hide built-in script field.
            };
            Editor.DrawPropertiesExcluding(this.serializedObject, excludedProperties);

            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(this.parallelDampingOnProp);
            EditorGUILayout.PropertyField(this.parallelElasticityOnProp);
            EditorGUILayout.PropertyField(this.autoBuildSegmentsProp);

            EditorGUILayout.LabelField("Muscle Path", EditorStyles.boldLabel);

            this.unfoldMuscleNodes = EditorGUILayout.Foldout(
                this.unfoldMuscleNodes,
                new GUIContent("Muscle Nodes"),
                true
            );

            if (this.unfoldMuscleNodes) {
                // Listen for changes to muscle nodes.
                EditorGUI.BeginChangeCheck();

                this.muscleNodesList.DoLayoutList();

                // Recalculate muscle data when muscle nodes are updated.
                if (EditorGUI.EndChangeCheck()) {
                    if (this.autoBuildSegmentsProp.boolValue) {
                        this.serializedObject.ApplyModifiedProperties();
                        foreach (MuscleTendonUnit target in this.targets) {
                            target.RefreshProperties();
                        }
                        muscleNodesProp.serializedObject.Update();
                    }
                }
            }

            EditorGUILayout.PropertyField(this.muscleSegmentsProp, true);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}