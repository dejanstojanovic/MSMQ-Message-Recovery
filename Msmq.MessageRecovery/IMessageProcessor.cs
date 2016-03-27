using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msmq.MessageRecovery
{
    public interface IMessageProcessor<T> where T : RecoverableMessage
    {
        bool ProcessMessage(T message);
        Exception GetProcessingException();
    }
}
