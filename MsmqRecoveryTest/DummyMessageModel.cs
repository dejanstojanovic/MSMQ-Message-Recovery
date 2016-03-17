using Msmq.MessageRecovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsmqRecoveryTest
{
    public class DummyMessageModel: RecoverableMessage
    {
        public Guid Key { get; set; }
    }
}
