using Msmq.MessageRecovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsmqRecoveryTest
{
    public class DummyMessageProcessor : IMessageProcessor
    {
        public Exception GetProcessingException()
        {
            return new ArgumentOutOfRangeException("Some dummy exception");
        }

        public bool ProcessMessage(RecoverableMessage message)
        {
            return false;
        }
    }
}
