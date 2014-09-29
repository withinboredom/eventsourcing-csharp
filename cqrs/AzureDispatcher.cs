using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace LiterallyFood.CQRS
{
    public class AzureDispatcher : MemoryDispatcher
    {
        QueueClient client;

        public AzureDispatcher(IEventStore store, QueueClient client)
            : base(store)
        {
            this.client = client;
        }

        public override async Task<bool> PublishEvent(object e)
        {
            var message = new BrokeredMessage(e);
            message.ContentType = e.GetType().AssemblyQualifiedName;
            await client.SendAsync(message);
            return true;
        }
    }
}
