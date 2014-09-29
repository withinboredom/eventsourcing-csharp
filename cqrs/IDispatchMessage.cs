using System;
using System.Threading.Tasks;
namespace LiterallyFood.CQRS
{
    interface IDispatchMessage
    {
        void AddHandlerFor<TCommand, TAggregate>() where TAggregate : Aggregate, new();
        void AddSubscriberFor<TEvent>(ISubscribeTo<TEvent> subscriber);
        void ScanAssembly(System.Reflection.Assembly ass);
        Task ScanInstance(object instance);
        Task SendCommand<TCommand>(TCommand c);
    }
}
