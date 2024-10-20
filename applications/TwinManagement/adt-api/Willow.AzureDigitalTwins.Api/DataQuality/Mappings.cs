using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.DataQuality.Model.Capability;
using Willow.DataQuality.Model.Validation;
using Willow.DataQuality.Model.ValidationResults;

namespace Willow.AzureDigitalTwins.Api.DataQuality;

public class Mappings : Profile
{
    public Mappings()
    {
        CreateMap<ValidationResults, ValidationResultsAdxDto>()
            .ForMember(m => m.ResultType, opt => opt.MapFrom(x => x.ResultType));

        CreateMap<CapabilityStatusDto, ValidationResultsAdxDto>()
          .ForMember(d => d.TwinDtId, s => s.MapFrom(x => x.TwinId))
          .ForMember(d => d.TwinIdentifiers, s => s.MapFrom(x =>
              JsonConvert.SerializeObject(new TwinIdentifiers(null, x.TrendId.ToString(), x.ConnectorId, x.ExternalId))))
            .ForMember(d => d.ResultSource, s => s.MapFrom(x => "RulesEngineCapabilityStatus"))
          .ForMember(d => d.ResultType, s => s.MapFrom(x => x.Status.Contains(StatusType.Ok) && x.Status.Count == 1 ? Result.Ok.ToString() : Result.Error.ToString()))
          .ForMember(d => d.ResultInfo, s => s.MapFrom((src, dest) =>
          {
              var c = new CapabilityStatus(src.TwinId, src.TrendId, src.ConnectorId, src.ExternalId, src.Status.Select(s => s.ToString()).ToList(), src.ReportedDateTime);
              object? destValue = JsonConvert.SerializeObject(c);
              return destValue;
          }))
          .ForMember(d => d.CheckType, s => s.MapFrom(src => "Telemetry"))
          .ForMember(d => d.RunInfo, s => s.MapFrom(x => JsonConvert.SerializeObject(new RunInfo(x.ReportedDateTime))));

        CreateMap<ValidationResultsAdxDto, ValidationResults>()
            .ForMember(m => m.ResultType, s => s.MapFrom<StringToEnumResolver, string>(src => src.ResultType))
            .ForMember(m => m.TwinIdentifiers, s =>
                s.MapFrom(x => JsonConvert.DeserializeObject(x.TwinIdentifiers.ToString(), typeof(TwinIdentifiers))))
            .ForMember(m => m.RunInfo, s => s.MapFrom(x => JsonConvert.DeserializeObject(x.RunInfo.ToString(), typeof(RunInfo))))
            .ForMember(m => m.RuleScope, s => s.MapFrom(x => JsonConvert.DeserializeObject(x.RuleScope.ToString(), typeof(RuleScope))))
            .ForMember(m => m.TwinInfo, s => s.MapFrom(x => JsonConvert.DeserializeObject(x.TwinInfo.ToString(), typeof(TwinInfo))))
            .ForMember(m => m.CheckType, s => s.MapFrom<StringToCheckEnumResolver, string>(src => src.CheckType))
            .ForMember(m => m.ResultInfo, s => s.MapFrom((src, dest) =>
            {
                object? destValue = null;
                //TODO: Need to handle rest of the CheckTypes
                switch (src.CheckType)
                {
                    case "Properties":
                        destValue = JsonConvert.DeserializeObject<List<ResultInfo>>(src.ResultInfo.ToString());
                        break;
                    case "Relationships":
                        destValue = JsonConvert.DeserializeObject<List<ResultInfoRelationship>>(src.ResultInfo.ToString());
                        break;
                    case "Telemetry":
                        destValue = JsonConvert.DeserializeObject<CapabilityStatus>(src.ResultInfo.ToString());
                        break;
                }
                return destValue;
            }));
    }
}

public class TwinIdentifiers
{
    public TwinIdentifiers() { }

    public TwinIdentifiers(string uniqueId, string trendId, string connectorId, string externalId)
    {
        this.uniqueId = uniqueId;
        this.trendId = trendId;
        this.connectorId = connectorId;
        this.externalId = externalId;
    }
    public string uniqueId { get; init; }
    public string trendId { get; init; }
    public string connectorId { get; init; }
    public string externalId { get; init; }
}

public record RuleScope(string type);

public record TwinInfo(Locations locations, string name);
public record Locations(Dictionary<string, string> location);

public record RunInfo(DateTime? CheckTime);
public record ResultInfo(string propertyName, PropertyValidationResultType type, string actualValue, string expectedValue);
public record CapabilityResultInfo(CapabilityStatusDto Result);
public record ResultInfoRelationship(bool IsValid, string Path);
public record CapabilityStatus(string? TwinId, Guid? TrendId, string? ConnectorId, string? ExternalId, List<string> Status, DateTime ReportedDateTime);



internal class StringToEnumResolver : IMemberValueResolver<object, object, string, Result>
{
    public Result Resolve(object source, object dest, string sourceMember, Result destMember, ResolutionContext context) =>
        (Result)Enum.Parse(typeof(Result), sourceMember);

}

internal class StringToCheckEnumResolver : IMemberValueResolver<object, object, string, CheckType>
{
    public CheckType Resolve(object source, object dest, string sourceMember, CheckType destMember, ResolutionContext context) =>
    (CheckType)Enum.Parse(typeof(CheckType), sourceMember);

}
