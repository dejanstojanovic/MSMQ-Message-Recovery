using Msmq.MessageRecovery.QueueManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Msmq.MessageRecovery
{

    public class MessageManager<T> where T : RecoverableMessage
    {
        #region Events

        public delegate void MessageEventHandler(T message);
        public delegate void MessageExceptionEventHandler(T message, Exception ex);

        /// <summary>
        /// Messsage processing failed and set for recovery
        /// </summary>
        public event MessageExceptionEventHandler ProcessingMessageFailed;

        /// <summary>
        /// Message processing faile with no recovery option
        /// </summary>
        public event MessageExceptionEventHandler ProcessingMessageFailedUnrecoverable;

        /// <summary>
        /// Message from processing queue processed
        /// </summary>
        public event MessageEventHandler ProcessingMessageSucceeded;

        /// <summary>
        /// Message took from recovery to processing queue
        /// </summary>
        public event MessageEventHandler ReprocessMessage;

        /// <summary>
        /// Maximum recovery count reached
        /// </summary>
        public event MessageEventHandler MaxReprocessCountReached;
        #endregion

        #region Fields
        TimeSpan recoveryDelay;
        RecoveryManager<T> recoveryManager;
        ProcessingManager<T> processingManager;
        int MaxRetryCount;


        #endregion

        #region Properties

        public List<Type> RecoverableExceptions
        {
            get; set;
        }

        #endregion

        #region Constructors
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

        #endregion

        #region Methods

        public void Add(T message)
        {
            this.processingManager.AddMessage(message);
        }

        #endregion



        private void RecoveryManager_ReprocessMessage(T message)
        {
            if (this.ReprocessMessage != null)
            {
                this.ReprocessMessage(message);
            }
            processingManager.AddMessage(message);
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
            if (this.RecoverableExceptions != null &&
                this.RecoverableExceptions.Any() &&
                !this.RecoverableExceptions.Contains(ex.GetType())) //Check if exception type is recoverable
            {
                if (this.ProcessingMessageFailedUnrecoverable != null)
                {
                    this.ProcessingMessageFailedUnrecoverable(message, ex);
                }
            }
            else {
                if (message.RetryCount < MaxRetryCount)
                {
                    if (this.ProcessingMessageFailed != null)
                    {
                        this.ProcessingMessageFailed(message, ex);
                    }
                    recoveryManager.AddMessage(message);
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
}
