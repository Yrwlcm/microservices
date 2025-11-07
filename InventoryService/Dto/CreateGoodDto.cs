namespace InventoryService.Dto;

public record CreateGoodDto(string Sku, string Name, int? Quantity);