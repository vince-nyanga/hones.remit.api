using System.Net;
using Hones.Remit.Api.Apis.Dtos.Orders;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Domain;
using Hones.Remit.Api.MassTransit.Commands.CancelOrder;
using Hones.Remit.Api.MassTransit.Events.OrderCollected;
using Hones.Remit.Api.MassTransit.Events.OrderExpired;
using Hones.Remit.Api.MassTransit.Events.OrderPaid;
using Hones.Remit.Api.MassTransit.Requests.CreateOrder;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Apis;

public static class OrdersApi
{
    public static void MapOrders(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/orders")
            .WithTags("Orders");

        group.MapGet("/", ApiHandler.GetAllOrders)
            .WithName("GetAllOrders")
            .Produces<List<OrderDto>>()
            .WithOpenApi();

        group.MapGet("/{orderId:guid}", ApiHandler.GetOrderById)
            .WithName("GetOrderById")
            .Produces<OrderDto>()
            .Produces<ProblemDetails>(statusCode: (int)HttpStatusCode.NotFound)
            .WithOpenApi();

        group.MapPost("/", ApiHandler.AddOrder)
            .WithName("AddOrder")
            .Accepts<CreateOrderDto>(contentType: "application/json")
            .Produces<OrderDto>(statusCode: (int)HttpStatusCode.Created)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/expire", ApiHandler.ExpireOrder)
            .WithName("ExpireOrder")
            .Produces((int)HttpStatusCode.OK)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
            .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/cancel", ApiHandler.CancelOrder)
            .WithName("CancelOrder")
            .Produces((int)HttpStatusCode.Accepted)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/pay", ApiHandler.PayOrder)
            .WithName("PayOrder")
            .Produces((int)HttpStatusCode.OK)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
            .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/collect", ApiHandler.CollectOrder)
            .WithName("CollectOrder")
            .Produces((int)HttpStatusCode.OK)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
            .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
            .WithOpenApi();
    }

    private sealed class ApiHandler
    {
        public static async Task<IResult> GetAllOrders(OrdersDbContext dbContext, CancellationToken cancellationToken)
        {
            var orders = await dbContext.Orders
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Results.Ok(orders.Select(MapToDto));
        }

        public static async Task<IResult> GetOrderById(OrdersDbContext dbContext, Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);

            return order is null
                ? Results.NotFound()
                : Results.Ok(MapToDto(order));
        }

        public static async Task<IResult> AddOrder(
            IRequestClient<CreateOrder> requestClient,
            CreateOrderDto createOrderDto,
            CancellationToken cancellationToken)
        {
            // This is an example of how to implement a request/response pattern using MassTransit
            var response = await requestClient.GetResponse<NewOrderResult, OrderCreationFailedResult>(new CreateOrder
            {
                SenderEmail = createOrderDto.SenderEmail,
                SenderName = createOrderDto.SenderName,
                RecipientEmail = createOrderDto.RecipientEmail,
                RecipientName = createOrderDto.RecipientName,
                Currency = createOrderDto.Currency,
                Amount = createOrderDto.Amount
            }, cancellationToken);
            
            if (response.Is(out Response<NewOrderResult>? newOrderResult))
            {
                var order = newOrderResult.Message;
                return Results.CreatedAtRoute("GetOrderById", new { orderId = order.Id }, order);
            }
           
            if (response.Is(out Response<OrderCreationFailedResult>? failedResult))
            {
                return Results.BadRequest(failedResult.Message.Error);
            }
            
            return Results.BadRequest("An error occurred while creating the order.");
        }

        public static async Task<IResult> ExpireOrder(
            OrdersDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(x => x.PublicId == orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.Expire();

            if (result.IsError)
            {
                return Results.BadRequest(result.FirstError.Description);
            }
            await publishEndpoint.Publish(new OrderExpired(order.PublicId), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok();
        }

        
        public static async Task<IResult> CancelOrder(
            ISendEndpointProvider sendEndpointProvider,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            // This is an example of how to send a message to a specific endpoint instead of publishing it to an exchange (RabbitMQ)
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:cancel-order"));
            await endpoint.Send(new CancelOrder(orderId), cancellationToken);
            return Results.Accepted();
        }
        
        public static async Task<IResult> PayOrder(
            OrdersDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(x => x.PublicId == orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.Pay();

            if (result.IsError)
            {
                return Results.BadRequest(result.FirstError.Description);
            }

            await publishEndpoint.Publish(new OrderPaid(order.PublicId), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok();
        }

        public static async Task<IResult> CollectOrder(
            OrdersDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(x => x.PublicId == orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.Collect();

            if (result.IsError)
            {
                return Results.BadRequest(result.FirstError.Description);
            }

            await publishEndpoint.Publish(new OrderCollected(order.PublicId), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok();
        }
        
        private static OrderDto MapToDto(Order orderModel)
        {
            return new OrderDto
            {
                Id = orderModel.PublicId,
                Status = orderModel.Status.ToString(),
                DateCreatedUtc = orderModel.DateCreatedUtc,
                DateExpiredUtc = orderModel.DateExpiredUtc,
                DatePaidUtc = orderModel.DatePaidUtc,
                DateCancelledUtc = orderModel.DateCancelledUtc,
                DateCollectedUtc = orderModel.DateCollectedUtc,
                SenderEmail = orderModel.SenderEmail,
                SenderName = orderModel.SenderName,
                RecipientEmail = orderModel.RecipientEmail,
                RecipientName = orderModel.RecipientName,
                Currency = orderModel.Currency,
                Amount = orderModel.Amount,
                Reference = EncodeId(orderModel.Id)
            };
        }

        private static string EncodeId(long id)
        {
            return Constants.Encoder.EncodeLong(id).ToUpperInvariant();
        }
    }
}