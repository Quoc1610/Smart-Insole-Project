using System;

namespace Kinesis {
    public class MuscleComponentLengthException : Exception {
        public MuscleComponentLengthException() {}

        public MuscleComponentLengthException(string message) : base(message) {}

        public MuscleComponentLengthException(string message, Exception inner) : base(message, inner) {}
    }
}