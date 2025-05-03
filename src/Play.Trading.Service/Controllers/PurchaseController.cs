using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Trading.Service.Contracts;

namespace Play.Trading.Service.Controllers
{
    [Route("purchase")]
    [ApiController]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPublishEndpoint publishEndpoint;

        public PurchaseController(IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
        {
            var userId = User.FindFirstValue("sub");

            var CorrelationId = Guid.NewGuid();
            var message = new PurchaseRequested(
                Guid.Parse(userId), purchase.ItemId.Value, purchase.Quantity, CorrelationId);

            await publishEndpoint.Publish(message);

            return Accepted();
        }
    }
}
