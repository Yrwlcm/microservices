namespace InventoryService.Dto;

public record CreateGoodDto(string Sku, string Name, decimal ItemPrice, int? Quantity);