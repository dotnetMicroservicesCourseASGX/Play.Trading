using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Trading.Service.Contracts;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Controllers
{
    [Route("purchase")]
    [ApiController]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IRequestClient<GetPurchaseState> purchaseClient;

        public PurchaseController(IPublishEndpoint publishEndpoint, IRequestClient<GetPurchaseState> purchaseClient)
        {
            this.publishEndpoint = publishEndpoint;
            this.purchaseClient = purchaseClient;
        }

        [HttpGet("status/{correlationId}")]
        public async Task<ActionResult<PurchaseDto>> GetStatusAsync(Guid CorrelationId)
        {
            var response = await purchaseClient.GetResponse<PurchaseState>(new GetPurchaseState(CorrelationId));
            var purchaseState = response.Message;
            var purchase = new PurchaseDto(
                purchaseState.UserId,
                purchaseState.ItemId,
                purchaseState.PurchaseTotal,
                purchaseState.Quantity,
                purchaseState.CurrentState,
                purchaseState.ErrorMessage,
                purchaseState.Received,
                purchaseState.LastUpdated);

            return Ok(purchase);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
        {
            var userId = User.FindFirstValue("sub");

            var CorrelationId = Guid.NewGuid();
            var message = new PurchaseRequested(
                Guid.Parse(userId), purchase.ItemId.Value, purchase.Quantity, CorrelationId);

            await publishEndpoint.Publish(message);

            return AcceptedAtAction(nameof(GetStatusAsync), new { correlationId = CorrelationId },
            new { correlationId = CorrelationId });
        }
    }
}
