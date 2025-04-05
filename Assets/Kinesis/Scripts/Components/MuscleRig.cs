using UnityEngine;

namespace Kinesis.Components {
    /// <summary>
    /// Component that automatically attaches/removes JointData components to Joint components. Meant to be attached
    // to the root of a skeleton, it will search children with Joint components to attach/remove JointData components.
    /// </summary>
    [ExecuteInEditMode]
    public class MuscleRig : MonoBehaviour {
        void OnDestroy() {
            JointData[] components = this.GetComponentsInChildren<JointData>();

            foreach (JointData c in components) {
                DestroyImmediate(c);
            }
        }

        void Reset() {
            Joint[] joints = this.GetComponentsInChildren<Joint>();

            foreach (Joint j in joints) {
                if (j.gameObject.GetComponent<JointData>() == null) {
                    j.gameObject.AddComponent<JointData>();
                }
            }
        }
    }
}