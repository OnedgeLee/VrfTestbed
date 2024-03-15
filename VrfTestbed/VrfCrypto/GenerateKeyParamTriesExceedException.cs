using System.Runtime.Serialization;

namespace VrfTestbed.VrfCrypto
{
    [Serializable]
    public class GenerateKeyParamTriesExceedException : Exception
    {
        public GenerateKeyParamTriesExceedException()
        {
        }

        public GenerateKeyParamTriesExceedException(string message)
          : base(message)
        {
        }

        public GenerateKeyParamTriesExceedException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        protected GenerateKeyParamTriesExceedException(
          SerializationInfo info, StreamingContext context
        )
          : base(info, context)
        {
        }
    }
}