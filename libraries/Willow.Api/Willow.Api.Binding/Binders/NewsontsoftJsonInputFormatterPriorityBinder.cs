namespace Willow.Api.Binding.Binders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A model binder that prioritizes the NewtonsoftJsonInputFormatter and NewtonsoftJsonPatchInputFormatter.
    /// </summary>
    public class NewsontsoftJsonInputFormatterPriorityBinder : IModelBinder
    {
        private readonly BodyModelBinder bodyModelBinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsontsoftJsonInputFormatterPriorityBinder"/> class.
        /// </summary>
        /// <param name="readerFactory">An instance of IHttpRequestStreamReaderFactory.</param>
        /// <param name="loggerFactory">An instance of ILoggerFactory.</param>
        /// <param name="options">The options.</param>
        public NewsontsoftJsonInputFormatterPriorityBinder(IHttpRequestStreamReaderFactory readerFactory, ILoggerFactory loggerFactory, IOptions<MvcOptions> options)
        {
            var formatters = options.Value.InputFormatters.ToList();

            PrioritizeFormatter(formatters, typeof(NewtonsoftJsonInputFormatter));
            PrioritizeFormatter(formatters, typeof(NewtonsoftJsonPatchInputFormatter));

            bodyModelBinder = new BodyModelBinder(formatters, readerFactory, loggerFactory, options.Value);
        }

        /// <summary>
        /// Binds the model.
        /// </summary>
        /// <param name="bindingContext">The model binding context.</param>
        /// <returns>An asynchronous task.</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            return bodyModelBinder.BindModelAsync(bindingContext);
        }

        private static void PrioritizeFormatter(List<IInputFormatter> formatters, Type type)
        {
            var index = formatters.FindIndex(x => x.GetType() == type);

            if (index > -1 && index != 0)
            {
                var formatter = formatters[index];
                formatters.RemoveAt(index);
                formatters.Insert(0, formatter);
            }
        }
    }
}
