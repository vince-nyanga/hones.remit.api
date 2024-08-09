using System.Net;
using System.Text;
using Hones.Remit.Api.Apis.Dtos.Orders;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Domain;
using Hones.Remit.Api.Messages.Commands;
using Hones.Remit.Api.Messages.Results;
using Hones.Remit.Api.Services;
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
            .Produces<ProblemDetails>(statusCode: (int)HttpStatusCode.BadRequest)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/expire", ApiHandler.ExpireOrder)
            .WithName("ExpireOrder")
            .Produces((int)HttpStatusCode.OK)
            .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/cancel", ApiHandler.CancelOrder)
            .WithName("CancelOrder")
            .Produces((int)HttpStatusCode.OK)
            .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/pay", ApiHandler.PayOrder)
            .WithName("PayOrder")
            .Produces((int)HttpStatusCode.OK)
            .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/collect", ApiHandler.CollectOrder)
            .WithName("CollectOrder")
            .Produces((int)HttpStatusCode.OK)
            .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
            .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
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
            OrdersDbContext dbContext,
            IRequestClient<CreateOrder> requestClient,
            CreateOrderDto createOrderDto,
            CancellationToken cancellationToken)
        {
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
            IEmailService emailService,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.Expire();
            await dbContext.SaveChangesAsync(cancellationToken);

            await result.SwitchAsync(
                _ => SendOrderExpiredEmail(emailService, order),
                _ => Task.CompletedTask);

            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
        }

        public static async Task<IResult> CancelOrder(
            OrdersDbContext dbContext,
            IEmailService emailService,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.Cancel();
            await dbContext.SaveChangesAsync(cancellationToken);

            await result.SwitchAsync(
                _ => SendOrderCancelledEmail(emailService, order),
                _ => Task.CompletedTask);

            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
        }
        
        public static async Task<IResult> PayOrder(
            OrdersDbContext dbContext,
            IEmailService emailService,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.Pay();
            await dbContext.SaveChangesAsync(cancellationToken);

            await result.SwitchAsync(
                _ => SendOrderPaidEmails(emailService, order),
                _ => Task.CompletedTask);

            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
        }

        public static async Task<IResult> CollectOrder(
            OrdersDbContext dbContext,
            IEmailService emailService,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.Collect();
            await dbContext.SaveChangesAsync(cancellationToken);

            await result.SwitchAsync(
                _ => SendOrderCollectedEmails(emailService, order),
                _ => Task.CompletedTask);

            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
        }

        private static async Task SendOrderExpiredEmail(IEmailService emailService, Order order)
        {
            var orderReference = EncodeId(order.Id);
            var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
                .AppendLine()
                .AppendLine()
                .AppendLine("Unfortunately your order has expired.")
                .AppendLine()
                .AppendLine("Order Details:")
                .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
                .AppendLine($"Reference: {orderReference}")
                .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
                .AppendLine()
                .AppendLine("Regards,")
                .AppendLine("HonesRemit Team");

            await emailService.SendEmailAsync(order.SenderEmail, $"Order Expired - {orderReference}",
                emailBuilder.ToString());
        }
        
        private static Task SendOrderCancelledEmail(IEmailService emailService, Order order)
        {
            var orderReference = EncodeId(order.Id);
            var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
                .AppendLine()
                .AppendLine()
                .AppendLine("You have successfully cancelled your order.")
                .AppendLine()
                .AppendLine("Order Details:")
                .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
                .AppendLine($"Reference: {orderReference}")
                .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
                .AppendLine()
                .AppendLine("Regards,")
                .AppendLine("HonesRemit Team");

            return emailService.SendEmailAsync(order.SenderEmail, $"Order Cancelled - {orderReference}",
                emailBuilder.ToString());
        }


        private static async Task SendOrderPaidEmails(IEmailService emailService, Order order)
        {
            var orderReference = EncodeId(order.Id);
            var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
                .AppendLine()
                .AppendLine()
                .AppendLine("Thank you for your payment. Your order is now ready for collection.")
                .AppendLine()
                .AppendLine("Order Details:")
                .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
                .AppendLine($"Reference: {orderReference}")
                .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
                .AppendLine()
                .AppendLine("Regards,")
                .AppendLine("HonesRemit Team");

            await emailService.SendEmailAsync(order.SenderEmail, $"Order ready for collection - {orderReference}",
                emailBuilder.ToString());


            emailBuilder = new StringBuilder($"Hi {order.RecipientName},")
                .AppendLine()
                .AppendLine()
                .AppendLine(
                    $"{order.SenderName} has sent you some money. Please go to your nearest HonesRemit collection point to collect.")
                .AppendLine()
                .AppendLine("Details:")
                .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
                .AppendLine($"Reference: {orderReference}")
                .AppendLine($"Sender: {order.SenderName} ({order.SenderEmail})")
                .AppendLine()
                .AppendLine("Regards,")
                .AppendLine("HonesRemit Team");

            await emailService.SendEmailAsync(order.RecipientEmail, $"You have received some money  - {orderReference}",
                emailBuilder.ToString());
        }
        
        private static async Task SendOrderCollectedEmails(IEmailService emailService, Order order)
        {
            var orderReference = EncodeId(order.Id);
            var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
                .AppendLine()
                .AppendLine()
                .AppendLine($"{order.RecipientName} has successfully collected the money.")
                .AppendLine()
                .AppendLine("Order Details:")
                .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
                .AppendLine($"Reference: {orderReference}")
                .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
                .AppendLine()
                .AppendLine("Regards,")
                .AppendLine("HonesRemit Team");

            await emailService.SendEmailAsync(order.SenderEmail, $"Order collected - {orderReference}",
                emailBuilder.ToString());

            emailBuilder = new StringBuilder($"Hi {order.RecipientName},")
                .AppendLine()
                .AppendLine()
                .AppendLine($"You have successfully collected the money sent by {order.SenderName}.")
                .AppendLine()
                .AppendLine("Details:")
                .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
                .AppendLine($"Reference: {orderReference}")
                .AppendLine($"Sender: {order.SenderName} ({order.SenderEmail})")
                .AppendLine()
                .AppendLine("Regards,")
                .AppendLine("HonesRemit Team");

            await emailService.SendEmailAsync(order.RecipientEmail, $"Money collected  - {orderReference}",
                emailBuilder.ToString());
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