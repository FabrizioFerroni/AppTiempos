using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppTiemposV3.Api.Utilidades;

public class TypeBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string? modelName = bindingContext.ModelName;
        ValueProviderResult valor = bindingContext.ValueProvider.GetValue(modelName);

        if (valor == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        try
        {
            Type tipoDestino = bindingContext.ModelMetadata.ModelType;
            object? valorDeserializado = JsonSerializer.Deserialize(valor.FirstValue!, tipoDestino,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            bindingContext.Result = ModelBindingResult.Success(valorDeserializado);
        }
        catch 
        {
            bindingContext.ModelState.TryAddModelError(modelName, "El valor dado no es del tipo de destino valido");
        }
        
        return Task.CompletedTask;
    }

}