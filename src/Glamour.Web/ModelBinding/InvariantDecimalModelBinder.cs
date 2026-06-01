using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Glamour.Web.ModelBinding;

/// <summary>
/// Faz o parsing de decimais usando InvariantCulture (ponto decimal), independente da
/// cultura da aplicação (pt-BR). Inputs HTML type="number" sempre enviam o valor com
/// ponto decimal, então parsear com a cultura pt-BR interpretaria o ponto como separador
/// de milhar, corrompendo o valor (ex.: 430.00 viraria 43000).
/// </summary>
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
            // Campo vazio: null para decimal?, sucesso sem valor para decimal
            if (ehNullable)
                bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // Aceita tanto ponto quanto vírgula como separador decimal, por robustez
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
