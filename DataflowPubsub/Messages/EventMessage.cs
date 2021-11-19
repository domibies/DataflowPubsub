namespace DataflowPubsub.Messages
{
    /// <summary>
    /// This is a simple message to represent an 'event'
    /// </summary>
    public class EventMessage : BaseMessage
    {
        public EventMessage(string eventId, string data = null, string topic = null) : base(topic)
        {
            EventId = eventId;
            Data = data;
        }

        public string EventId { get; private set; }
        public string Data { get; private set; }

        public override BaseMessage DeepCopy()
        {
            var message = new EventMessage(EventId, Data, Topic);
            message.SetCopiedFrom(this);
            return message;
        }
    }
}
