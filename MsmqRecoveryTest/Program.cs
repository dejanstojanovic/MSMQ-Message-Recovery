using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using Msmq.MessageRecovery;
using Msmq.MessageRecovery.Formatters;
using Msmq.MessageRecovery.QueueManagers;

namespace MsmqRecoveryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string processQueuePath = @".\private$\messages.toprocess";
            string recoveryQueuePath = @".\private$\messages.torecover";


            var recMgr = new MessageManager<DummyMessageModel>(
                processQueuePath,
                recoveryQueuePath,
                new DummyMessageProcessor(),
                TimeSpan.FromSeconds(5),
                3
                );

            recMgr.RecoverableExceptions = new List<Type>() {
                typeof(ArgumentNullException),
                typeof(ArgumentOutOfRangeException)
            };

            recMgr.ProcessingMessageFailedUnrecoverable += RecMgr_ProcessingMessageFailedUnrecoverable;
            recMgr.ProcessingMessageFailed += RecMgr_ProcessingMessageFailed;
            recMgr.ReprocessMessage += RecMgr_ReprocessMessage;
            recMgr.MaxReprocessCountReached += RecMgr_MaxReprocessCountReached;

            for (int i = 0; i < 1; i++)
            {
                recMgr.Add(new DummyMessageModel() { Key = Guid.NewGuid() });
            }

            Console.ReadLine();
        }

     

        private static void RecMgr_ProcessingMessageFailed(DummyMessageModel message, Exception ex)
        {
            Console.WriteLine("{0} Message processing failed", DateTime.Now.ToLongTimeString());
        }

        private static void RecMgr_ReprocessMessage(DummyMessageModel message)
        {
            Console.WriteLine("{0} Reprocessing message from recovery queue", DateTime.Now.ToLongTimeString());
        }

        private static void RecMgr_MaxReprocessCountReached(DummyMessageModel message)
        {
            Console.WriteLine("{0} Max number of recovery attemts reached", DateTime.Now.ToLongTimeString());
        }

        private static void RecMgr_ProcessingMessageFailedUnrecoverable(DummyMessageModel message, Exception ex)
        {
            Console.WriteLine("{0} UNRECOVERABLE EXCEPTION OCCURED", DateTime.Now.ToLongTimeString());
        }
    }
}
