using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace Msmq.MessageRecovery.QueueManagers
{

    
    internal class RecoveryManager<T> : IDisposable where T : RecoverableMessage
    {
        public delegate void ReprocessMessageHandler(T message);
        public event ReprocessMessageHandler ReprocessMessage;

        private TimeSpan messageProcessDelay;
        public TimeSpan MessageProcessDelay
        {
            get
            {
                return messageProcessDelay;
            }
        }

        private String messageQueuePath;
        private String MessageQueuePath
        {
            get
            {
                return messageQueuePath;
            }
        }

        private bool isDisposing = false;
        private MessageQueue recoveryQueue;
        CancellationTokenSource cancelTokenSource;
        CancellationToken cancelToken;

        public RecoveryManager(string messageQueuePath, TimeSpan messageProcessDelay)
        {
            cancelTokenSource = new CancellationTokenSource();
            cancelToken = cancelTokenSource.Token;

            this.messageQueuePath = messageQueuePath;
            this.messageProcessDelay = messageProcessDelay;

            recoveryQueue = new MessageQueue(this.messageQueuePath);
            recoveryQueue.Formatter = new Formatters.JsonMessageFormatter<T>();
            recoveryQueue.MessageReadPropertyFilter.ArrivedTime = true;
            recoveryQueue.DefaultPropertiesToSend.Priority = MessagePriority.High;

            Task.Factory.StartNew(() =>
            {
                MessageEnumerator enumerator = recoveryQueue.GetMessageEnumerator2();

                while (!cancelToken.IsCancellationRequested)
                {

                    while (enumerator.MoveNext())
                    {
                        Message recoveryMessage = recoveryQueue.Peek();

                        if (recoveryMessage != null && DateTime.Now - recoveryMessage.ArrivedTime >= this.messageProcessDelay)
                        {
                            Message recoveryProcessMessage = recoveryQueue.ReceiveById(recoveryMessage.Id);
                            if (recoveryProcessMessage != null)
                            {
                                T message = recoveryMessage.Body as T;
                                //Put back to processing que
                                if (message != null && ReprocessMessage != null)
                                {
                                    ReprocessMessage(message);
                                }
                            }
                        }
                    }

                    enumerator.Reset();

                    Thread.Sleep(this.messageProcessDelay);
                }

            }, cancelToken);
        }

        public void AddMessage(T message)
        {
            //Increase the count
            message.IncreaseRetryCount();
            this.recoveryQueue.Send(message);
        }

        public void Dispose()
        {
            if (!isDisposing)
            {
                cancelTokenSource.Cancel();
                isDisposing = true;
            }
        }
    }
}
