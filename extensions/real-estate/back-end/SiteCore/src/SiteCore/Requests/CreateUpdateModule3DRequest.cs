using System.Collections.Generic;
using FluentValidation;

namespace SiteCore.Requests
{
    public class CreateUpdateModule3DRequest
    {
        public List<Module3DInfo> Modules3D { get; set; } = new List<Module3DInfo>();
    }

    public class Module3DInfo
    {
        public string Url { get; set; }

        public string ModuleName { get; set; }
    }

    public class Module3DInfoValidator : AbstractValidator<Module3DInfo>
    {
        public Module3DInfoValidator()
        {
            RuleFor(x => x.ModuleName).NotNull().NotEmpty();
            RuleFor(x => x.Url).NotNull().NotEmpty().MaximumLength(1024);
        }
    }

    public class CreateUpdateModule3DRequestValidator : AbstractValidator<CreateUpdateModule3DRequest>
    {
        public CreateUpdateModule3DRequestValidator()
        {
            RuleFor(x => x.Modules3D).NotEmpty();
            RuleForEach(x => x.Modules3D).SetValidator(new Module3DInfoValidator());
        }
    }
}
