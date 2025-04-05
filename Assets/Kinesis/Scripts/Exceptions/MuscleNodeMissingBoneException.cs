using System;

namespace Kinesis {
    public class MuscleNodeMissingBoneException : Exception {
        public MuscleNodeMissingBoneException() {}

        public MuscleNodeMissingBoneException(string message) : base(message) {}

        public MuscleNodeMissingBoneException(string message, Exception inner) : base(message, inner) {}
    }
}