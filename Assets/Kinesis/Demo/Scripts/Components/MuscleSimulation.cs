using Kinesis.Components;
using Kinesis.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Kinesis.Demo {
    public class MuscleSimulation : MonoBehaviour {
        public bool simulationOn = true;
        private Dictionary<MuscleTendonUnit, Dictionary<MuscleSegment, Vector3>> muscleJointTorquesMap;
        private Dictionary<MuscleTendonUnit, MuscleStimulator> muscleStimulatorMap;

        void FixedUpdate() {
            if (!this.simulationOn) {
                return;
            }

            foreach (KeyValuePair<MuscleTendonUnit, Dictionary<MuscleSegment, Vector3>> kvp in
                this.muscleJointTorquesMap) {
                MuscleTendonUnit muscle = kvp.Key;
                Dictionary<MuscleSegment, Vector3> jointTorques = kvp.Value;

                // Check for excitation signal from Muscle Stimulator component.
                MuscleStimulator muscleStimulator;
                this.muscleStimulatorMap.TryGetValue(muscle, out muscleStimulator);
                float excitation = (muscleStimulator != null) ? muscleStimulator.excitation : 0.0f;

                muscle.CalculateJointTorques(excitation, ref jointTorques);
                MuscleTendonUnit.ApplyJointTorques(jointTorques);
            }
        }

        void Awake() {
            Rigidbody[] rigidbodies = GameObject.FindObjectsOfType<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies) {
                // Adjustment for rigid body stability issues.
                rb.inertiaTensor *= 20.0f;
            }

            this.muscleStimulatorMap = new Dictionary<MuscleTendonUnit, MuscleStimulator>();

            // Initialize array of all joint torque calculation result maps.
            MuscleTendonUnit[] muscles = GameObject.FindObjectsOfType<MuscleTendonUnit>();
            this.muscleJointTorquesMap = new Dictionary<MuscleTendonUnit, Dictionary<MuscleSegment, Vector3>>();

            foreach (MuscleTendonUnit muscle in muscles) {
                // Initialize joint torque calculation result map for the specific muscle.
                this.muscleJointTorquesMap[muscle] = new Dictionary<MuscleSegment, Vector3>();

                // Map muscle stimulators to muscles.
                MuscleStimulator muscleStimulator = muscle.GetComponent<MuscleStimulator>();
                if (muscleStimulator != null) {
                    this.muscleStimulatorMap.Add(muscle, muscleStimulator);
                }
            }
        }
    }
}