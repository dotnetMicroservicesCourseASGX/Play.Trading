using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Controllers
{
    [Route("store")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IRepository<InventoryItem> _inventoryItemRepository;

        public StoreController(IRepository<CatalogItem> catalogItemRepository, IRepository<ApplicationUser> userRepository, IRepository<InventoryItem> inventoryItemRepository)
        {
            _catalogItemRepository = catalogItemRepository;
            _userRepository = userRepository;
            _inventoryItemRepository = inventoryItemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<StoreDto>> GetAsync()
        {
            var userId = User.FindFirstValue("sub");
            var catalogItems = await _catalogItemRepository.GetAllAsync();
            var inventoryItems = await _inventoryItemRepository.GetAllAsync(item => item.UserId == Guid.Parse(userId));

            var user = await _userRepository.GetAsync(Guid.Parse(userId));
            if (user == null)
            {
                return NotFound("User not found.");
            }


            var storeDto = new StoreDto(
                catalogItems.Select(catalogItem => new StoreItemDto(catalogItem.Id, catalogItem.Name,
                catalogItem.Description, catalogItem.Price,
                inventoryItems.FirstOrDefault(inventoryItem =>
                 inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0)),
                user?.Gil ?? 0
            );

            return Ok(storeDto);
        }
    }
}
