using Msmq.MessageRecovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsmqRecoveryTest
{
    public class DummyMessageProcessor : IMessageProcessor<DummyMessageModel>
    {
        public Exception GetProcessingException()
        {
            return new ArgumentOutOfRangeException("Some dummy exception");
        }

        public bool ProcessMessage(DummyMessageModel message)
        {
            throw new NotImplementedException();
        }

    }
}
