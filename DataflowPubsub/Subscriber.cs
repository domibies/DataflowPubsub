using DataflowPubsub.Messages;
using System;
using System.Threading.Tasks.Dataflow;

namespace DataflowPubsub
{
    /// <summary>
    /// Encapsulates a subscription to a message type T that derives from BaseMessage.
    /// The application can read from the subscriber through the exposed 'Receiver'
    /// property. A subscriber can be created independently and connected to a source of 
    /// 'BaseMessage'. But usually, the subscriber is created by an instance of 'MessageBus'.
    /// </summary>
    /// <typeparam name="T">The type to subscribe to</typeparam>
    public class Subscriber<T> : IDisposable where T : BaseMessage
    {
        /// <summary>
        /// The exposed 'Receiver' that can be read/received from
        /// </summary>
        public ISourceBlock<T> Receiver => copyToDerived; 
        // this block casts & copies the message instances & fucntions as the internal buffer
        private readonly TransformBlock<BaseMessage, T> copyToDerived;
        private bool disposed;
        private IDisposable sourceLink = null;
        /// <summary>
        /// Creates a subscriber that can read from any 'BaseMessage' source. Only the instances
        /// That are actually of the (derived) type 'T' will be copied
        /// </summary>
        /// <param name="source">The source to link from</param>
        /// <param name="filter">The optional subscription filter</param>
        public Subscriber(ISourceBlock<BaseMessage> source, Predicate<T> filter = null)
        {
            copyToDerived = new TransformBlock<BaseMessage, T>((o) => (T)o.DeepCopy());

            _linkFrom(source, filter);
        }

        // internal method that defines the link. This filters away the instances that aren't
        // of type 'T', or don't fulfill the optional subscription predicate
        private void _linkFrom(ISourceBlock<BaseMessage> source, Predicate<T> filter = null)
        {
            // conditional link = message is of type T AND (optional) filter
            sourceLink = source.LinkTo(copyToDerived, (m) => (m is T) && (filter == null ? true : filter((T)m)));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (sourceLink != null)
                    {
                        sourceLink.Dispose();
                    }
                }

                disposed = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
