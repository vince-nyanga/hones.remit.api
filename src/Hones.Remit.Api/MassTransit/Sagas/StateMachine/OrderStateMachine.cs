using Hones.Remit.Api.MassTransit.Events.OrderCancelled;
using Hones.Remit.Api.MassTransit.Events.OrderCollected;
using Hones.Remit.Api.MassTransit.Events.OrderCreated;
using Hones.Remit.Api.MassTransit.Events.OrderExpired;
using Hones.Remit.Api.MassTransit.Events.OrderPaid;
using Hones.Remit.Api.MassTransit.Events.OrderReadyForCollection;
using Hones.Remit.Api.MassTransit.Events.OrderTimedOut;
using Hones.Remit.Api.MassTransit.Events.ValidatePayment;
using Hones.Remit.Api.MassTransit.Events.ValidationSucceeded;
using MassTransit;

namespace Hones.Remit.Api.MassTransit.Sagas.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        
        Schedule(() => PaymentTimeout, x => x.PaymentTimeoutTokenId, x =>
        {
            x.Delay = TimeSpan.FromSeconds(30);
            x.Received = e => e.CorrelateById(context => context.Message.OrderId);
        });
        
        Event(() => OrderCreated, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderPaid, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCancelled, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCollected, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderReadyForCollection, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentValidationSucceeded, x => x.CorrelateById(context => context.Message.OrderId));
        
        Initially(
            When(OrderCreated)
                .Then(context =>
                {
                    context.Saga.DateCreatedUtc = DateTimeOffset.UtcNow;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CurrentState = OrderState.States.Created;
                })
                .Schedule(PaymentTimeout, context => new OrderExpired(context.Saga.OrderId))
                .TransitionTo(Created));
        
        During(Created,
            When(OrderPaid)
                .ThenAsync( async context =>
                {
                    context.Saga.DatePaidUtc = DateTimeOffset.UtcNow;
                    context.Saga.CurrentState = OrderState.States.Paid;
                    await context.Publish(new ValidatePayment(context.Saga.OrderId));
                })
                .Unschedule(PaymentTimeout)
                .TransitionTo(Paid),
            When(OrderCancelled)
                .Then(context =>
                {
                    context.Saga.DateCancelledUtc = DateTimeOffset.UtcNow;
                    context.Saga.CurrentState = OrderState.States.Cancelled;
                })
                .TransitionTo(Cancelled)
                .Finalize(),
            When(PaymentTimeout!.Received)
                .ThenAsync(async context =>
                {
                    context.Saga.DateExpiredUtc = DateTimeOffset.UtcNow;
                    context.Saga.CurrentState = OrderState.States.Expired;
                    await context.Publish(new OrderTimedOut(context.Saga.OrderId));
                })
                .TransitionTo(Expired)
                .Finalize());
        
        During(Paid,
            When(PaymentValidationSucceeded)
                .Then(context =>
                {
                    context.Saga.DateValidationSucceededUtc = DateTimeOffset.UtcNow;
                    context.Saga.DateReadyForCollection = DateTimeOffset.UtcNow;
                    context.Saga.CurrentState = OrderState.States.ReadyForCollection;
                })
                .TransitionTo(ReadyForCollection));
            
        During(ReadyForCollection,
            When(OrderCollected)
                .Then(context =>
                {
                    context.Saga.DateCollectedUtc = DateTimeOffset.UtcNow;
                    context.Saga.CurrentState = OrderState.States.Collected;
                })
                .TransitionTo(Collected)
                .Finalize());
    }
    
    public State? Created { get; private set; }
    public State? Paid { get; private set; }
    public State? Cancelled { get; private set; }
    public State? ReadyForCollection { get; private set; }
    public State? Collected { get; private set; }
    public State? Expired { get; private set; }

    public State? ValidationSuceeded { get; set; }
    
    public Schedule<OrderState, OrderExpired>? PaymentTimeout { get; private set; }
    
    public Event<OrderCreated>? OrderCreated { get; private set; }
    public Event<OrderPaid>? OrderPaid { get; private set; }
    public Event<OrderCancelled>? OrderCancelled { get; private set; }
    public Event<OrderCollected>? OrderCollected { get; private set; }
    public Event<OrderReadyForCollection>? OrderReadyForCollection { get; private set; }
    public Event<PaymentValidationSucceeded>? PaymentValidationSucceeded { get; private set; }
}