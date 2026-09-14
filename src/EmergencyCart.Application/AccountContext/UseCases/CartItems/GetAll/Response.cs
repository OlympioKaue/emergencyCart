namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.GetAll;

public sealed record class Response(Guid id, Guid itemId, string itemName, string category, int expectedQuantity);


