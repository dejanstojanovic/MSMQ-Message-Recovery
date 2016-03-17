using Msmq.MessageRecovery.QueueManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msmq.MessageRecovery
{

    public class MessageManager<T> where T : RecoverableMessage
    {
        #region Events
        public delegate void MessageEventHandler(T message);
        public delegate void MessageExceptionEventHandler(T message, Exception ex);

        public event MessageExceptionEventHandler ProcessingMessageFailed;
        public event MessageEventHandler ProcessingMessageSucceeded;
        public event MessageEventHandler ReprocessMessage;
        public event MessageEventHandler MaxReprocessCountReached;
        #endregion

        #region Fields
        TimeSpan recoveryDelay;
        RecoveryManager<T> recoveryManager;
        ProcessingManager<T> processingManager;
        int MaxRetryCount;
        #endregion

        public MessageManager(String receivingQueuePath, String recoveryQueuePath, IMessageProcessor messageProcessor) :
            this(receivingQueuePath, recoveryQueuePath, messageProcessor, TimeSpan.FromMinutes(1), 3)
        {

        }
        public MessageManager(String receivingQueuePath, String recoveryQueuePath, IMessageProcessor messageProcessor, TimeSpan recoveryDelay, int maxRetryCount, int numberOfReadingTasks = 1)
        {
            if (messageProcessor == null)
                throw new ArgumentNullException("messageProcessor", "messageProcessor cannot be null");

            if (maxRetryCount <= 0)
                throw new ArgumentOutOfRangeException("maxRetryCount", "maxRetryCount must be an integer value starting from 1");

            this.recoveryDelay = recoveryDelay;
            this.MaxRetryCount = maxRetryCount;
            processingManager = new ProcessingManager<T>(receivingQueuePath, messageProcessor, numberOfReadingTasks);
            processingManager.MessageProcessingFailed += ProcessingManager_MessageProcessingFailed;
            processingManager.MessageProcessed += ProcessingManager_MessageProcessed;

            recoveryManager = new RecoveryManager<T>(recoveryQueuePath, this.recoveryDelay);
            recoveryManager.ReprocessMessage += RecoveryManager_ReprocessMessage;
        }

        public void Add(T message)
        {
            this.processingManager.AddMessage(message);
        }

        private void RecoveryManager_ReprocessMessage(T message)
        {
            processingManager.AddMessage(message);
            if (this.ReprocessMessage != null)
            {
                this.ReprocessMessage(message);
            }
        }

        private void ProcessingManager_MessageProcessed(T message)
        {
            if (this.ProcessingMessageSucceeded != null)
            {
                this.ProcessingMessageSucceeded(message);
            }
        }

        private void ProcessingManager_MessageProcessingFailed(T message, Exception ex)
        {

            if (message.RetryCount < MaxRetryCount)
            {
                recoveryManager.AddMessage(message);
                if (this.ProcessingMessageFailed != null)
                {
                    this.ProcessingMessageFailed(message, ex);
                }
            }
            else
            {
                //Max retry count excided
                if (this.MaxReprocessCountReached != null)
                {
                    this.MaxReprocessCountReached(message);
                }
            }
        }
    }
}
