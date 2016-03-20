# MSMQ-Message-Recovery

Simple event driven concept of MSMQ message retry with a delay option and non-recoverable exception type using two MSMQs

##How does it work
The solution is based on two message queues wher one is processing queue and another one is a recovery queue. The recovery can be limited only to certain types of exceptions and with delay meaning that reovery will wait for certain amount of time to try to reprocess message.

##How to use
Since it is event driven, all you need to do is to create an instance of MessageManager class and add handlers to it's events.
To create an instance you need to pass the instance which implements IProcessor interface which will be used to process the actual message.
This class will be your custom logic processor class which will handle message picked up from the processing queue.
After adding a message using "Add" method, the object will we handled by the flow, passed to processor an in case of failure it will be put for recovery based on the recovery options passed in the MessageManages class onstructor.
