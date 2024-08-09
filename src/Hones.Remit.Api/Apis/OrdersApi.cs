using System.Net;
using Hones.Remit.Api.Apis.Dtos.Orders;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Domain;
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
        
        public static async Task<IResult> GetOrderById(OrdersDbContext dbContext, Guid orderId, CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);
            
            return order is null 
                ? Results.NotFound() 
                : Results.Ok(MapToDto(order));
        }
        
        public static async Task<IResult> AddOrder(OrdersDbContext dbContext, CreateOrderDto createOrderDto, CancellationToken cancellationToken)
        {
            var orderResult = Order.Create(
                createOrderDto.SenderEmail,
                createOrderDto.SenderName,
                createOrderDto.RecipientEmail,
                createOrderDto.RecipientName,
                createOrderDto.Currency,
                createOrderDto.Amount
            );
            
            if (orderResult.IsError)
            {
                // TODO: return error details
                return Results.BadRequest();
            }
            
            var order = orderResult.Value;
            
            await dbContext.Orders.AddAsync(order, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return Results.CreatedAtRoute("GetOrderById", new { orderId = order.PublicId }, MapToDto(order));
        }
        
        public static async Task<IResult> ExpireOrder(OrdersDbContext dbContext, Guid orderId, CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);
            
            if (order is null)
            {
                return Results.NotFound();
            }
            
            var result = order.Expire();
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
        }
        
        public static async Task<IResult> CancelOrder(OrdersDbContext dbContext, Guid orderId, CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);
            
            if (order is null)
            {
                return Results.NotFound();
            }
            
            var result = order.Cancel();
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
        }
        
        public static async Task<IResult> PayOrder(OrdersDbContext dbContext, Guid orderId, CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);
            
            if (order is null)
            {
                return Results.NotFound();
            }
            
            var result = order.Pay();
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
        }
        
        public static async Task<IResult> CollectOrder(OrdersDbContext dbContext, Guid orderId, CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.PublicId == orderId, cancellationToken);
            
            if (order is null)
            {
                return Results.NotFound();
            }
            
            var result = order.Collect();
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return result.MatchFirst(
                _ => Results.Ok(),
                error => Results.BadRequest(error.Description)
            );
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
                Amount = orderModel.Amount
            };
        }
    }
}