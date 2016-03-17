using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msmq.MessageRecovery
{
    public interface IMessageProcessor
    {
        bool ProcessMessage(RecoverableMessage message);
        Exception GetProcessingException();
    }
}
