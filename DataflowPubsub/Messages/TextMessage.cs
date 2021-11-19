namespace DataflowPubsub.Messages
{
    /// <summary>
    /// This is a simple message to hold any string value
    /// </summary>
    public class TextMessage : BaseMessage
    {
        public TextMessage(string value, string topic = null) : base(topic)
        {
            Value = value;
        }

        public string Value { get; protected set; }
        public override BaseMessage DeepCopy()
        {
            var newMessage = new TextMessage(Value);
            // allways do this when creating a copy
            SetCopiedFrom(this);
            return newMessage;
        }
    }
}
