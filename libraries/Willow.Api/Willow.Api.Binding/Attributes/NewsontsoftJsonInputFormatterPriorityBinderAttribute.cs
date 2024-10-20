namespace Willow.Api.Binding.Attributes
{
    using Microsoft.AspNetCore.Mvc;
    using Willow.Api.Binding.Binders;

    /// <summary>
    /// The newsontsoft json input formatter priority binder attribute.
    /// </summary>
    public class NewsontsoftJsonInputFormatterPriorityBinderAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewsontsoftJsonInputFormatterPriorityBinderAttribute"/> class.
        /// </summary>
        public NewsontsoftJsonInputFormatterPriorityBinderAttribute()
            : base(typeof(NewsontsoftJsonInputFormatterPriorityBinder))
        {
            BindingSource = Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Body;
        }
    }
}
