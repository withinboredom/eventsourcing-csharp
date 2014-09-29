using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEventStore;

namespace LiterallyFood.CQRS
{
    public class INEventStore : IEventStore, IDisposable
    {
        private IStoreEvents store;
        private string connectionString;

        public INEventStore(string connection)
        {
            connectionString = connection;

            store = Wireup.Init()
                .LogToOutputWindow()
                .UsingSqlPersistence(connection)
                .WithDialect(new NEventStore.Persistence.Sql.SqlDialects.MsSqlDialect())
                .InitializeStorageEngine()
                .UsingJsonSerialization()
                .UsingAsynchronousDispatchScheduler()
                .DispatchTo(new NEventStore.Dispatcher.DelegateMessageDispatcher(DispatchCommit))
                .Build();
        }

        protected void DispatchCommit(NEventStore.ICommit commit)
        {
            // should send to azure?
        }

        public System.Collections.IEnumerable LoadEventsFor<TAggregate>(Guid id)
        {
            var list = new List<object>();

            try
            {
                using (IEventStream stream = store.OpenStream(id, 0, int.MaxValue))
                {
                    foreach (var body in stream.CommittedEvents)
                    {
                        list.Add(body.Body);
                    }
                }
            }
            catch (Exception ex)
            {
                // nothing found
            }

            return list;
        }

        public void SaveEventsFor<TAggregate>(Guid id, int eventsLoaded, System.Collections.ArrayList newEvents)
        {
            Dictionary<string, object> headers;

            using (IEventStream stream = store.OpenStream(id, 0, int.MaxValue))
            {
                foreach (var @event in newEvents)
                {
                    if (eventsLoaded != stream.StreamRevision)
                    {
                        throw new Exception("Concurrancy conflict - please retry.");
                    }

                    headers = new Dictionary<string,object>();
                    headers.Add("type", (typeof(TAggregate)).AssemblyQualifiedName);

                    stream.Add(new EventMessage
                         {
                             Headers = headers,
                             Body = @event
                         });

                    stream.CommitChanges(Guid.NewGuid());
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return;
        }

        protected virtual void Dispose(bool disposing)
        {
            store.Dispose();
        }
    }
}
