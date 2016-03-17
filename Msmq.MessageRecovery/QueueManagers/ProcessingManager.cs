using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Msmq.MessageRecovery.QueueManagers
{

    internal class ProcessingManager<T> : IDisposable where T : RecoverableMessage
    {
        public delegate void ProcessingMessageHandler(T message);
        public delegate void ProcessingFaileMessageHandler(T message, Exception ex);

        public event ProcessingMessageHandler MessageProcessed;
        public event ProcessingFaileMessageHandler MessageProcessingFailed;

        private String messageQueuePath;
        private String MessageQueuePath
        {
            get
            {
                return messageQueuePath;
            }
        }


        private int numberOfReadingTasks = 1;
        private int NumberOfReadingTasks
        {
            get
            {
                return numberOfReadingTasks;
            }
        }

        private bool isDisposing = false;

        private MessageQueue processingQueue;
        CancellationTokenSource cancelTokenSource;
        CancellationToken cancelToken;
        private IMessageProcessor messageProcessor;


        public ProcessingManager(string messageQueuePath, IMessageProcessor messageProcessor, int numberOfReadingTasks = 1)
        {
            if (numberOfReadingTasks < 1)
                throw new ArgumentOutOfRangeException("numberOfReadingTasks", "You need to have at least one task for reding from the processing queue");

            
            cancelTokenSource = new CancellationTokenSource();
            cancelToken = cancelTokenSource.Token;

            this.messageProcessor = messageProcessor;
            this.numberOfReadingTasks = numberOfReadingTasks;
            this.messageQueuePath = messageQueuePath;
            processingQueue = new MessageQueue(this.messageQueuePath);
            processingQueue.Formatter = new Formatters.JsonMessageFormatter<T>();


            for (int t = 0; t < this.numberOfReadingTasks; t++)
            {
                Task.Factory.StartNew(() =>
                {
                    while (!cancelToken.IsCancellationRequested)
                    {
                        Message processingMessage = processingQueue.Receive();

                        if (processingMessage != null)
                        {
                            T message = processingQueue.Formatter.Read(processingMessage) as T;

                            if (message != null)
                            {
                                if (!messageProcessor.ProcessMessage(message))
                                {
                                //Put to recovery queue
                                if (MessageProcessingFailed != null)
                                    {
                                        MessageProcessingFailed(message, messageProcessor.GetProcessingException());
                                    }

                                }
                                else
                                {
                                //Processing succeeded 
                                if (MessageProcessed != null)
                                    {
                                        MessageProcessed(message);
                                    }
                                }

                            }
                        }
                    }
                }, cancelToken);
            }
        }


        public void AddMessage(T message)
        {
            this.processingQueue.Send(message);
        }

        public void Dispose()
        {
            if (!isDisposing)
            {
                isDisposing = true;
            }
        }
    }
}
