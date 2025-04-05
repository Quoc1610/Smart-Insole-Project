using System;
using UnityEngine;

namespace Kinesis.Core {
    /// <summary>
    /// MuscleSegment represents a linear segment of a muscle path. A MuscleSegment can also be associated with a joint
    /// that it "spans" (i.e. the segment nodes are on separate, adjoining bones).
    /// </summary>
    /// <remarks>
    /// Note: A single muscle segment should not span across multiple joints.
    /// </remarks>
    [Serializable]
    public class MuscleSegment {
        /// <summary>
        /// The "next" MuscleSegment toward the muscle origin (if exists).
        /// </summary>
        [SerializeReference]
        public MuscleSegment prevSegment;
        /// <summary>
        /// MuscleTendonUnit that the segment is a part of.
        /// </summary>
        public MuscleTendonUnit parentMuscle;
        /// <summary>
        /// MuscleNode representing the origin-oriented terminal of the segment.
        /// </summary>
        [SerializeReference]
        public MuscleNode head;
        /// <summary>
        /// MuscleNode representing the insertion-oriented terminal of the segment.
        /// </summary>
        [SerializeReference]
        public MuscleNode tail;
        /// <summary>
        /// Unity Joint spanned by the segment (if exists).
        /// </summary>
        public Joint joint;
        /// <summary>
        /// Unity Rigidbody to apply joint torques to.
        /// </summary>
        public Rigidbody jointAnchorBody;
    }
}