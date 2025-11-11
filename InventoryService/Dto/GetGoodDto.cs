namespace InventoryService.Dto;

public record GetGoodDto(string Sku, string Name, int Quantity, int ItemPrice);