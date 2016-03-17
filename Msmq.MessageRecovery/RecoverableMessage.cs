using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msmq.MessageRecovery
{
    public abstract class RecoverableMessage
    {
        public DateTime OriginalTimeCreated { get; set;}
        public int RetryCount { get; set; }
        

        public RecoverableMessage()
        {
            
        }

        public void IncreaseRetryCount()
        {
            this.RetryCount += 1;
        }
    }
}
