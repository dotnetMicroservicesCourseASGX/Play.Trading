using System;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Consumers;

public class InventoryItemUpdatedConsumer : IConsumer<InventoryItemUpdated>
{
    private readonly IRepository<InventoryItem> _inventoryRepository;

    public InventoryItemUpdatedConsumer(IRepository<InventoryItem> inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task Consume(ConsumeContext<InventoryItemUpdated> context)
    {
        var message = context.Message;

        var inventoryItem = await _inventoryRepository.GetAsync(item => 
            item.Id == message.UserId && item.CatalogItemId == message.CatalogItemId);

        if(inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                UserId = message.UserId,
                CatalogItemId = message.CatalogItemId,
                Quantity = message.NewTotalQuantity
            };
            await _inventoryRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity = message.NewTotalQuantity;
            await _inventoryRepository.UpdateAsync(inventoryItem);
        }
    }
}