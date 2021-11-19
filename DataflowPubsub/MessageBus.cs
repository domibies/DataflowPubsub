using DataflowPubsub.Messages;
using System;
using System.Threading.Tasks.Dataflow;

namespace DataflowPubsub
{
    /// <summary>
    /// We van write anything of type 'BaseMessage' to the MessageBus. Subscribers, which are created
    /// By the bus can than be used to read subscribed messages from
    /// </summary>
    public class MessageBus
    {
        // the actual internal 'queue' that all messages are writen to
        private BufferBlock<BaseMessage> writeQueue { get; set; }
        /// <summary>
        /// Exposed by the bus from sending messages to
        /// </summary>
        public ITargetBlock<BaseMessage> Sender => writeQueue;
        // the broadcastter to distribute to all active subscribers
        private readonly BroadcastBlock<BaseMessage> _broadcastBlock;

        public MessageBus()
        {
            writeQueue = new BufferBlock<BaseMessage>();
            _broadcastBlock = new BroadcastBlock<BaseMessage>((m) => m, new DataflowBlockOptions { EnsureOrdered = true }); ;
            writeQueue.LinkTo(_broadcastBlock); // this way the queue distributes to all listeners
        }
        /// <summary>
        /// Creates a subscriber for a certain type, derived from BaseMessage. Only messages 
        /// of that type will be ex^posed from the subscriber. An optional predicate can be 
        /// used to only receive message that fulfill certain criterai (e.g. of a certain 'topic')
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        /// <param name="filter">The predicate that's used as a subscription filter (can be null)</param>
        /// <returns>The disposable subscriber instance</returns>
        public Subscriber<T> CreateSubscriber<T>(Predicate<T> filter = null) where T : BaseMessage
        {
            return new Subscriber<T>(_broadcastBlock, filter);
        }
    }
}
