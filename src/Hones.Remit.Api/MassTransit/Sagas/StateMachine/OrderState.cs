using MassTransit;

namespace Hones.Remit.Api.MassTransit.Sagas.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid? PaymentTimeoutTokenId { get; set; }
    public required string CurrentState { get; set; }
    public DateTimeOffset? DateCreatedUtc { get; set; }
    public DateTimeOffset? DatePaidUtc { get; set; }
    public DateTimeOffset? DateCancelledUtc { get; set; }
    public DateTimeOffset? DateReadyForCollection { get; set; }
    public DateTimeOffset? DateCollectedUtc { get; set; }
    public DateTimeOffset? DateExpiredUtc { get; set; }
    public DateTimeOffset? DateValidationSucceededUtc { get; set; }

    internal static class States
    {
        public const string Created = "Created";
        public const string Paid = "Paid";
        public const string Cancelled = "Cancelled";
        public const string ReadyForCollection = "ReadyForCollection";
        public const string Collected = "Collected";
        public const string Expired = "Expired";
        public const string ValidationSucceeded = "ValidationSucceeded";
    }
}