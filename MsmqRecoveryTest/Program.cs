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
            string recoveryQueuePath = @".\private$\messages.recovery";


            var recMgr = new MessageManager<DummyMessageModel>(
                processQueuePath,
                recoveryQueuePath,
                new DummyMessageProcessor(),
                TimeSpan.FromSeconds(10),
                3
                );

            for (int i = 0; i < 1; i++)
            {
                recMgr.Add(new DummyMessageModel() { Key = Guid.NewGuid() });
            }

            Console.ReadLine();
        }
    }
}
