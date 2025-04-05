using Kinesis.Core;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace Kinesis.Components {
    /// <summary>
    /// Component for testing individual muscles from the Unity interface.
    /// </summary>
    [RequireComponent(typeof(MuscleTendonUnit))]
    [DisallowMultipleComponent]
    public class MuscleStimulator : MonoBehaviour {
        public const float MinExcitation = 0.0f;
        public const float MaxExcitation = 1.0f;
        public bool runFixedUpdate;
        [Range(MinExcitation, MaxExcitation)]
        public float excitation;
        public MuscleTendonUnit muscle;
        private Dictionary<MuscleSegment, Vector3> jointTorques;

        void Awake() {
            this.jointTorques = new Dictionary<MuscleSegment, Vector3>();
        }

        void FixedUpdate() {
            if (!runFixedUpdate) {
                return;
            }

            this.muscle.CalculateJointTorques(this.excitation, ref this.jointTorques);
            MuscleTendonUnit.ApplyJointTorques(this.jointTorques);
        }

        void Reset() {
            this.muscle = this.gameObject.GetComponent<MuscleTendonUnit>();
        }
    }
}
#endif