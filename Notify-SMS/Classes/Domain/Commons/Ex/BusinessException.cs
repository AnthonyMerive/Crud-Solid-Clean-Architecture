using System.Runtime.Serialization;

namespace Classes.Domain.Commons.Ex
{
    [Serializable]
    public class BusinessException : Exception, ISerializable
    {
        public BusinessException(string message) : base(message)
        {
        }

        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
