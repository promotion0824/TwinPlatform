using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using Newtonsoft.Json;

namespace SiteCore.Requests
{
    public class CreateLayerGroupRequest
    {
        public string Name { get; set; }

        public bool Is3D { get; set; }

        public List<CreateEquipmentRequest> Equipments { get; set; } = new List<CreateEquipmentRequest>();

        public List<CreateZoneRequest> Zones { get; set; } = new List<CreateZoneRequest>();

        public List<CreateLayerRequest> Layers { get; set; } = new List<CreateLayerRequest>();
    }

    public class CreateEquipmentRequest
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();
    }

    public class CreateZoneRequest
    {
        public bool Is3D { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
    }

    public class CreateLayerRequest
    {
        public string Name { get; set; }

        public string TagName { get; set; }
    }

    public class CreateLayerGroupRequestValidator : AbstractValidator<CreateLayerGroupRequest>
    {
        public CreateLayerGroupRequestValidator()
        {
            RuleForEach(x => x.Layers).Must(IsLayerTagNameUnique)
                .WithMessage((request, layerRequest) => 
                    $"[{layerRequest.TagName}] tag name is duplicated in list of layers");

            RuleForEach(x => x.Zones).Must(IsEquipmentIdsUnique)
                .WithMessage((request, zoneRequest) => 
                    $"Zone with geometry {JsonConvert.SerializeObject(zoneRequest.Geometry)} contains equipment ids that are also attached to another zone");
        }

        private static bool IsEquipmentIdsUnique(
            CreateLayerGroupRequest groupRequest, 
            CreateZoneRequest zoneRequest, 
            PropertyValidatorContext context)
        {
            var otherEquipmentIds = groupRequest.Zones.Where(z => z != zoneRequest).SelectMany(z => z.EquipmentIds);
            var intersection = otherEquipmentIds.Intersect(zoneRequest.EquipmentIds);
            return !intersection.Any();
        }

        private static bool IsLayerTagNameUnique(
            CreateLayerGroupRequest groupRequest, 
            CreateLayerRequest layerRequest, 
            PropertyValidatorContext context)
        {
            var uniq = !groupRequest.Layers.Any(l => layerRequest != l && layerRequest.TagName == l.TagName);
            return uniq;
        }
    }
}
