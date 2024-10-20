using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace SiteCore.Requests
{
    public class UpdateLayerGroupRequest
    {
        public string Name { get; set; }

        public bool Is3D { get; set; }

        public List<UpdateEquipmentRequest> Equipments { get; set; } = new List<UpdateEquipmentRequest>();

        public List<UpdateZoneRequest> Zones { get; set; } = new List<UpdateZoneRequest>();

        public List<UpdateLayerRequest> Layers { get; set; } = new List<UpdateLayerRequest>();
    }

    public class UpdateEquipmentRequest
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();
    }

    public class UpdateZoneRequest
    {
        public Guid? Id { get; set; }

        public bool Is3D { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
    }

    public class UpdateLayerRequest
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string TagName { get; set; }
    }

    public class UpdateLayerGroupRequestValidator : AbstractValidator<UpdateLayerGroupRequest>
    {
        public UpdateLayerGroupRequestValidator()
        {
            RuleForEach(x => x.Layers).Must(IsLayerTagNameUnique)
                .WithMessage((request, layerRequest) => 
                    $"[{layerRequest.TagName}] tag name is duplicated in list of layers");

            RuleForEach(x => x.Zones).Must(IsEquipmentIdsUnique)
                .WithMessage((request, zoneRequest) => 
                    $"Zone [{zoneRequest.Id}] contains equipment ids that are also attached to another zone");
        }

        private static bool IsLayerTagNameUnique(
            UpdateLayerGroupRequest groupRequest, 
            UpdateLayerRequest layerRequest, 
            PropertyValidatorContext context)
        {
            var uniq = !groupRequest.Layers.Any(l => layerRequest != l && layerRequest.TagName == l.TagName);
            return uniq;
        }

        private static bool IsEquipmentIdsUnique(
            UpdateLayerGroupRequest groupRequest, 
            UpdateZoneRequest zoneRequest, 
            PropertyValidatorContext context)
        {
            var otherEquipmentIds = groupRequest.Zones.Where(z => z != zoneRequest).SelectMany(z => z.EquipmentIds);
            var intersection = otherEquipmentIds.Intersect(zoneRequest.EquipmentIds);
            return !intersection.Any();
        }
    }
}
