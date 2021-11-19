using DataflowPubsub;
using DataflowPubsub.Messages;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace TestProject
{
    public class TestMessageBus
    {
        [Fact]
        public async Task MessageBus_OneSubscriberReceives()
        {
            var bus = new MessageBus();
            var subscriber = bus.CreateSubscriber<TextMessage>();

            await bus.Sender.SendAsync(new TextMessage("test"));

            var receivedValue = string.Empty;
            var cancelSource = new CancellationTokenSource();

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

            await Task.Delay(100);
            cancelSource.Cancel();
            await taskSubscriber;

            Assert.Equal("test", receivedValue);
        }

        [Fact]
        public async Task MessageBus_DiposingSubscriberWorks()
        {
            var bus = new MessageBus();
            var subscriberToDipose = bus.CreateSubscriber<TextMessage>();
            var anotherSubscriber = bus.CreateSubscriber<TextMessage>();
            subscriberToDipose.Dispose(); // this should break the link with the messagebus

            await bus.Sender.SendAsync(new TextMessage("test"));

            var receivedValue = string.Empty;
            var cancelSource = new CancellationTokenSource();

            var taskAnotherSubscriber = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var textMessage = await anotherSubscriber.Receiver.ReceiveAsync(cancelSource.Token);
                        receivedValue = textMessage.Value;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });

            var noDisposedReceive = false;
            var taskDisposedSubscriber = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var textMessage = await subscriberToDipose.Receiver.ReceiveAsync(cancelSource.Token);
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                    noDisposedReceive = true;
                }
            });

            await Task.Delay(100);
            cancelSource.Cancel();
            await taskAnotherSubscriber;
            await taskDisposedSubscriber;

            Assert.Equal("test", receivedValue);
            Assert.True(noDisposedReceive);
        }

        [Fact]
        public async Task MessageBus_TwoSubscribersReceive()
        {
            var bus = new MessageBus();
            var subscriber1 = bus.CreateSubscriber<TextMessage>();
            var subscriber2 = bus.CreateSubscriber<TextMessage>();

            await bus.Sender.SendAsync(new TextMessage("test"));

            var receivedValue1 = string.Empty;
            var receivedValue2 = string.Empty;
            var cancelSource = new CancellationTokenSource();

            var taskSubscriber1 = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var textMessage = await subscriber1.Receiver.ReceiveAsync(cancelSource.Token);
                        receivedValue1 = textMessage.Value;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });
            var taskSubscriber2 = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var textMessage = await subscriber2.Receiver.ReceiveAsync(cancelSource.Token);
                        receivedValue2 = textMessage.Value;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });

            await Task.Delay(100);
            cancelSource.Cancel();
            await taskSubscriber1;
            await taskSubscriber2;

            Assert.Equal("test", receivedValue1);
            Assert.Equal("test", receivedValue2);
        }

        [Fact]
        public async Task MessageBus_UnsubscribedMessagesIgnored()
        {
            var bus = new MessageBus();
            var subscriber = bus.CreateSubscriber<TextMessage>();

            await bus.Sender.SendAsync(new BinaryMessage(Encoding.Default.GetBytes("ignore")));
            await bus.Sender.SendAsync(new TextMessage("test"));

            var receivedValue = string.Empty;
            var cancelSource = new CancellationTokenSource();

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

            await Task.Delay(100);
            cancelSource.Cancel();
            await taskSubscriber;

            Assert.Equal("test", receivedValue);
        }

        [Fact]
        public async Task MessageBus_SubscribeWithFilter()
        {
            // create a message bus
            var bus = new MessageBus();

            // this subscriber only needs to receive TextMessages that can be parsed as an integer
            // and are a plural of 3
            var subscriber = bus.CreateSubscriber<TextMessage>(
                (m) => Int32.TryParse(m.Value, out var result) && result % 3 == 0);

            // the susbcriber will not receive this one
            await bus.Sender.SendAsync(new TextMessage("not_an_integer"));
            // the subscriber will receive some of these 
            for (int i = 0; i < 10; i++)
            {
                await bus.Sender.SendAsync(new TextMessage($"{i}"));
            }

            int countMessages = 0;
            var cancelSource = new CancellationTokenSource();

            var taskSubscriber = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        // we wil receive "0", "3", "6" and "9"
                        var textMessage = await subscriber.Receiver.ReceiveAsync(cancelSource.Token);
                        countMessages++;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });

            await Task.Delay(100);
            cancelSource.Cancel();
            await taskSubscriber;

            Assert.Equal(4, countMessages);
        }

        [Fact]
        public async Task MessageBus_EventMessageAndTopicFiltering_Works()
        {
            var bus = new MessageBus();

            var subscriberRequest = bus.CreateSubscriber<EventMessage>((m) => m.Topic == "Request");
            var subscriberResponse = bus.CreateSubscriber<EventMessage>((m) => m.Topic == "Response");

            var cancelSource = new CancellationTokenSource();
            var taskHandleRequest = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var request = await subscriberRequest.Receiver.ReceiveAsync(cancelSource.Token);
                        Assert.Equal("Question", request.EventId);
                        Assert.Equal("Request", request.Topic);
                        Assert.Equal("Time", request.Data);
                        await bus.Sender.SendAsync(new EventMessage("Answer", $"{DateTime.Now:s}", "Response"), cancelSource.Token);
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });
            int countResponses = 0;
            var taskHandleResponse = Task.Run(async () =>
            {
                try
                {
                    while (!cancelSource.IsCancellationRequested)
                    {
                        var response = await subscriberResponse.Receiver.ReceiveAsync(cancelSource.Token);
                        Assert.Equal("Response", response.Topic);
                        Assert.Equal("Answer", response.EventId);
                        Assert.True(DateTime.TryParse(response.Data, out var result));
                        countResponses++;
                    }

                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });
            for (int i = 0; i < 5; ++i)
            {
                await bus.Sender.SendAsync(new EventMessage("Question", "Time", "Request"), cancelSource.Token);
            }
            await Task.Delay(100);
            cancelSource.Cancel();
            await taskHandleRequest;
            await taskHandleResponse;
            Assert.Equal(5, countResponses);
        }
    }
}
