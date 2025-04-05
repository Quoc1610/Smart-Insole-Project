using Kinesis.Core;
using UnityEngine;

namespace Kinesis.Components {
    /// <summary>
    /// Container component for MuscleTendonUnit objects.
    /// </summary>
    public class MuscleGroup : MonoBehaviour {
        // Call muscle property refresh on all child muscle objects.
        public void Refresh() {
            MuscleTendonUnit[] muscles = GetComponentsInChildren<MuscleTendonUnit>();
            foreach (MuscleTendonUnit muscle in muscles) {
                muscle.RefreshProperties();
            }
        }
    }
}