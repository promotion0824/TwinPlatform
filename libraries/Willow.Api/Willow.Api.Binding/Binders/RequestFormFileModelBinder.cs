namespace Willow.Api.Binding.Binders
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// The request form file model binder.
    /// </summary>
    public class RequestFormFileModelBinder : IModelBinder
    {
        private readonly IOptions<MvcNewtonsoftJsonOptions> jsonOptions;
        private readonly FormFileModelBinder formFileModelBinder;

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFormFileModelBinder"/> class.
        /// </summary>
        /// <param name="jsonOptions">The jsonOptions instance to use for all instances of this binder.</param>
        /// <param name="loggerFactory">An ILoggerFactory instance.</param>
        public RequestFormFileModelBinder(IOptions<MvcNewtonsoftJsonOptions> jsonOptions, ILoggerFactory loggerFactory)
        {
            this.jsonOptions = jsonOptions;
            formFileModelBinder = new FormFileModelBinder(loggerFactory);
        }

        /// <summary>
        /// Bind the model from the binding context.
        /// </summary>
        /// <param name="bindingContext">The input binding context.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            // Retrieve the form part containing the JSON
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);
            if (valueResult == ValueProviderResult.None)
            {
                // The JSON was not found
                var message = bindingContext.ModelMetadata.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(bindingContext.FieldName);
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, message);
                return;
            }

            var rawValue = valueResult.FirstValue;

            // Deserialize the JSON
            var model = JsonSerializer.Deserialize(rawValue, bindingContext.ModelType, Options);

            // Now, bind each of the IFormFile properties from the other form parts
            foreach (var property in bindingContext.ModelMetadata.Properties)
            {
                if (property.ModelType != typeof(IFormFile))
                {
                    continue;
                }

                var fieldName = property.BinderModelName ?? property.PropertyName;
                var modelName = fieldName;
                var propertyModel = property.PropertyGetter(bindingContext.Model);
                ModelBindingResult propertyResult;
                using (bindingContext.EnterNestedScope(property, fieldName, modelName, propertyModel))
                {
                    await formFileModelBinder.BindModelAsync(bindingContext);
                    propertyResult = bindingContext.Result;
                }

                if (propertyResult.IsModelSet)
                {
                    // The IFormFile was sucessfully bound, assign it to the corresponding property of the model
                    property.PropertySetter(model, (FormFile)propertyResult.Model);
                }
                else if (property.IsBindingRequired)
                {
                    var message = property.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(fieldName);
                    bindingContext.ModelState.TryAddModelError(modelName, message);
                }
            }

            // Set the successfully constructed model as the result of the model binding
            bindingContext.Result = ModelBindingResult.Success(model);
        }
    }
}
