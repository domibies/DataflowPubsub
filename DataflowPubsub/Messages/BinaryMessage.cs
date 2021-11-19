using System.Linq;

namespace DataflowPubsub.Messages
{
    /// <summary>
    /// This is a simple message to hold any byte[]
    /// </summary>
    public class BinaryMessage : BaseMessage
    {
        public BinaryMessage(byte[] value, string topic = null) : base(topic)
        {
            // don't keep a reference, but create a copy of the byte []
            Value = value.ToArray();
        }

        /// <summary>
        /// The actual content of the message
        /// </summary>
        public byte[] Value { get; protected set; }
        public override BaseMessage DeepCopy()
        {
            var newMessage = new BinaryMessage(Value);
            // allways do this when creating a copy
            SetCopiedFrom(this);
            return newMessage;
        }
    }
}
