using System;

namespace DataflowPubsub.Messages
{
    /// <summary>
    /// Abstract base class for any message that is sent trough the 
    /// MessageBus. 
    /// </summary>
    public abstract class BaseMessage
    {
        public const string DefaultTopic = "Default";

        /// <summary>
        /// Globally unique Uid for a message instance
        /// </summary>
        public Guid Uid { get; private set; }
        /// <summary>
        /// Time when the message was initally created
        /// </summary>
        public DateTime Created { get; private set; }
        /// <summary>
        /// When a derived message is created, the message keeps
        /// a reference to the Uid of the original message (the
        /// first one in the chain
        /// </summary>
        public Guid CorrelationId { get; private set; }
        /// <summary>
        /// Any message can have a 'topic' to subscribe on
        /// </summary>
        public string Topic { get; private set; }


        public BaseMessage(string topic)
        {
            Topic = topic ?? DefaultTopic;
            Uid = Guid.NewGuid();
            Created = DateTime.Now;
            CorrelationId = Uid;
        }

        /// <summary>
        /// Call this when creating a derived message
        /// </summary>
        /// <param name="original"></param>
        public void SetDerivedFrom(BaseMessage original)
        {
            if (Uid == original.Uid)
            {
                // if it's a copy, we give this a new identity
                Uid = Guid.NewGuid();
                Created = DateTime.Now;
            }
            CorrelationId = original.CorrelationId;
        }

        /// <summary>
        /// Creates a derived message with a new topic
        /// </summary>
        /// <param name="topic"></param>
        public virtual void CreateWithNewTopic(string topic)
        {
            var copy = DeepCopy();
            copy.SetDerivedFrom(this);
            Topic = topic;
        }

        /// <summary>
        /// This is called when making a copy of a message
        /// </summary>
        /// <param name="original"></param>
        protected void SetCopiedFrom(BaseMessage original)
        {
            Uid = original.Uid;
            CorrelationId = original.CorrelationId;
            Created = original.Created;
        }

        /// <summary>
        /// Each message type need to supply this method, so
        /// copies can be created where needed
        /// </summary>
        /// <returns></returns>
        public abstract BaseMessage DeepCopy();
    }
}
