using DataflowPubsub;
using DataflowPubsub.Messages;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace TestProject
{
    public class TestSubscriber
    {
        [Fact]
        public async Task SubcriberOfType_ReceivesSameType()
        {
            var bufferBlock = new BufferBlock<BaseMessage>();
            var subscriber = new Subscriber<TextMessage>(bufferBlock);
            bufferBlock.LinkTo(DataflowBlock.NullTarget<BaseMessage>()); // needed to catch the messages that arent't filtered to the subscriber

            await bufferBlock.SendAsync(new TextMessage("Hello"));

            var cancelSource = new CancellationTokenSource();
            var receivedValue = string.Empty;

            var taskSubscriber = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var textMessage = await subscriber.Receiver.ReceiveAsync(cancelSource.Token);
                        receivedValue = textMessage.Value;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });


            // make sure we don't cancel the task before it can receive
            await Task.Delay(100);

            cancelSource.Cancel();
            await taskSubscriber;

            Assert.Equal("Hello", receivedValue);
        }

        [Fact]
        public async Task SubcriberOfType_DoesntReceiveOtherType()
        {
            var bufferBlock = new BufferBlock<BaseMessage>();
            var subscriber = new Subscriber<TextMessage>(bufferBlock);
            bufferBlock.LinkTo(DataflowBlock.NullTarget<BaseMessage>()); // needed to catch the messages that arent't filtered to the subscriber

            await bufferBlock.SendAsync(new BinaryMessage(Encoding.Default.GetBytes("Hello")));

            var cancelSource = new CancellationTokenSource();
            var receivedValue = string.Empty;
            bool cancelled = false;

            var taskSubscriber = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var textMessage = await subscriber.Receiver.ReceiveAsync(cancelSource.Token);
                        receivedValue = textMessage.Value;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                    cancelled = true;
                }
            });


            // make sure we don't cancel the task before it can receive
            await Task.Delay(100);

            cancelSource.Cancel();
            await taskSubscriber;

            Assert.Equal(string.Empty, receivedValue);
            Assert.True(cancelled);
        }

        [Fact]
        public async Task SubcriberOfType_SkipsOtherType()
        {
            var bufferBlock = new BufferBlock<BaseMessage>();
            var subscriber = new Subscriber<TextMessage>(bufferBlock);
            bufferBlock.LinkTo(DataflowBlock.NullTarget<BaseMessage>()); // needed to catch the messages that arent't filtered to the subscriber


            for (int i = 0; i < 10; ++i)
            {
                await bufferBlock.SendAsync(new BinaryMessage(Encoding.Default.GetBytes("NotThis"))); // shouldn't be recieved
                await bufferBlock.SendAsync(new TextMessage($"Hello{i}")); // should be received
            }

            var cancelSource = new CancellationTokenSource();
            var receivedValue = string.Empty;
            int receiveCount = 0;

            var taskSubscriber = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var textMessage = await subscriber.Receiver.ReceiveAsync(cancelSource.Token);
                        receivedValue = textMessage.Value;
                        ++receiveCount;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });


            // make sure we don't cancel the task before it can receive
            await Task.Delay(100);

            cancelSource.Cancel();
            await taskSubscriber;

            Assert.Equal("Hello9", receivedValue);
            Assert.Equal(10, receiveCount);
        }
    }
}