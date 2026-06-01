using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Glamour.Web.ModelBinding;

public class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

        var valor = valueResult.FirstValue;
        var ehNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;

        if (string.IsNullOrWhiteSpace(valor))
        {

            if (ehNullable)
                bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        var normalizado = valor.Trim().Replace(",", ".");

        if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultado))
        {
            bindingContext.Result = ModelBindingResult.Success(resultado);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Valor numérico inválido.");
        }

        return Task.CompletedTask;
    }
}

public class InvariantDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var t = context.Metadata.UnderlyingOrModelType;
        return t == typeof(decimal) ? new InvariantDecimalModelBinder() : null;
    }
}
