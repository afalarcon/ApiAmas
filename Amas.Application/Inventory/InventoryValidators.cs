using FluentValidation;

namespace Amas.Application.Inventory;

public sealed class CreateInventoryItemRequestValidator : AbstractValidator<CreateInventoryItemRequest>
{
    public CreateInventoryItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Type).Must(BeSupportedItemType).WithMessage("Inventory type must be Product, Supply or Element.");
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(40);
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
    }

    private static bool BeSupportedItemType(string value) =>
        value.Trim().Equals("Product", StringComparison.OrdinalIgnoreCase) ||
        value.Trim().Equals("Supply", StringComparison.OrdinalIgnoreCase) ||
        value.Trim().Equals("Element", StringComparison.OrdinalIgnoreCase);
}

public sealed class UpdateInventoryItemRequestValidator : AbstractValidator<UpdateInventoryItemRequest>
{
    public UpdateInventoryItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Type).Must(BeSupportedItemType).WithMessage("Inventory type must be Product, Supply or Element.");
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(40);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
    }

    private static bool BeSupportedItemType(string value) =>
        value.Trim().Equals("Product", StringComparison.OrdinalIgnoreCase) ||
        value.Trim().Equals("Supply", StringComparison.OrdinalIgnoreCase) ||
        value.Trim().Equals("Element", StringComparison.OrdinalIgnoreCase);
}

public sealed class CreateInventoryMovementRequestValidator : AbstractValidator<CreateInventoryMovementRequest>
{
    public CreateInventoryMovementRequestValidator()
    {
        RuleFor(x => x.MovementType).Must(BeSupportedMovementType).WithMessage("Movement type must be Entry or Exit.");
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).When(x => x.UnitCost.HasValue);
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.Reference).MaximumLength(160);
    }

    private static bool BeSupportedMovementType(string value) =>
        value.Trim().Equals("Entry", StringComparison.OrdinalIgnoreCase) ||
        value.Trim().Equals("Entrada", StringComparison.OrdinalIgnoreCase) ||
        value.Trim().Equals("Exit", StringComparison.OrdinalIgnoreCase) ||
        value.Trim().Equals("Salida", StringComparison.OrdinalIgnoreCase);
}
