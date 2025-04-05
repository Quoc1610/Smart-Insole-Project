using System;
using UnityEngine;

namespace Kinesis.Core {
    /// <summary>
    /// MuscleNode represents a single point that forms part of a muscle path. Each MuscleNode is defined in relation to a
    /// specific "bone" represented as a Unity GameObject.
    /// </summary>
    [Serializable]
    public class MuscleNode {
        /// <summary>
        /// Unity GameObject of the body that the muscle node remains relative to. The GameObject should have a
        /// Rigidbody component attached.
        /// </summary>
        public GameObject bone;
        /// <summary>
        /// Position offset relative to bone position.
        /// </summary>
        public Vector3 offset;
    }
}