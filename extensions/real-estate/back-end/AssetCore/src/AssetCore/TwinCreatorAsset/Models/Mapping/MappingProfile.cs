using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.Features.Asset.Attachments.Models;
using AssetCoreTwinCreator.Features.Asset.Search;
using AssetCoreTwinCreator.MappingId.Extensions;
using AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Domain.Models.Asset, Models.Asset>()
                .ForMember(dest => dest.AssetParameters, options => options.MapFrom(src => new List<Models.AssetParameter>()))
                .ForMember(dest => dest.ParentCategoryId, options => options.MapFrom(src => src.Category != null ? src.Category.ParentId : null));

            CreateMap<Models.Asset, Domain.Models.Asset>()
                .ForMember(dest => dest.SpaceId, options => options.Ignore());

            CreateMap<Domain.Models.Category, Models.Category>()
                .ForMember(dest => dest.HasTemplateAssigned, options => options.MapFrom(src => !string.IsNullOrEmpty(src.DbTableName)))
                .ForMember(dest => dest.GroupIds, options => options.MapFrom(src => src.CategoryGroups.Select(cg => cg.GroupId)));

            CreateMap<Models.Category, Domain.Models.Category>()
                .ForMember(dest => dest.DbObjectId, options => options.Ignore())
                .ForMember(dest => dest.DbTableName, options => options.Ignore());

            CreateMap<Models.CategoryColumn, Domain.Models.CategoryColumn>()
                .ForMember(dest => dest.DataTypeEnum, options => options.MapFrom(src => src.DataType))
                .ForMember(dest => dest.OnValidationErrorEnum, options => options.MapFrom(src => src.OnValidationError))
                .ForMember(dest => dest.AllowedValues, options => options.MapFrom(src => RemoveSpacesForCommaSeparatedString(src.AllowedValues)));

            CreateMap<Models.Category, CategoryDto>()
                .ForMember(dest => dest.SiteId, options => options.Ignore())
                .ForMember(dest => dest.Id, options => options.Ignore())
                .ForMember(dest => dest.ParentId, options => options.Ignore())
                .ForMember(dest => dest.ChildCategories, options => options.Ignore());

            CreateMap<Models.CategoryColumn, CategoryColumnDto>()
                .ForMember(dest => dest.CategoryId, options => options.Ignore())
                .ForMember(dest => dest.Id, options => options.Ignore());

            CreateMap<Models.Asset, AssetSimpleDto>()
                .ForMember(dest => dest.CategoryId, options => options.Ignore())
                .ForMember(dest => dest.SiteId, options => options.Ignore())
                .ForMember(dest => dest.FloorId, options => options.Ignore())
                .ForMember(dest => dest.CompanyId, options => options.Ignore())
                .ForMember(dest => dest.ParentCategoryId, options => options.Ignore())
                .ForMember(dest => dest.Id, options => options.Ignore());

            CreateMap<Models.Asset, AssetDto>()
                .ForMember(dest => dest.CategoryId, options => options.Ignore())
                .ForMember(dest => dest.SiteId, options => options.Ignore())
                .ForMember(dest => dest.FloorId, options => options.Ignore())
                .ForMember(dest => dest.CompanyId, options => options.Ignore())
                .ForMember(dest => dest.ParentCategoryId, options => options.Ignore())
                .ForMember(dest => dest.Id, options => options.Ignore());

            CreateMap<AssetSearchParametersDto, AssetSearchParameters>()
                .ForMember(dest => dest.FilterByFloorCode, options => options.Ignore())
                .ForMember(dest => dest.FilterByAssetRegisterIds,
                    options => options.MapFrom(src =>
                        src.FilterByAssetRegisterIds.Select(g => g.ToAssetId()).ToList()));

            CreateMap<File, FileDto>()
                .ForMember(dest => dest.Id, options => options.MapFrom(src => src.Id.ToFileGuid()))
                .ForMember(dest => dest.AssetIds, options => options.MapFrom(src => src.AssetRegisterIds == null ? null : src.AssetRegisterIds.Select(id => id.ToAssetGuid()).ToList()));


        }

        private static string RemoveSpacesForCommaSeparatedString(string inputString)
        {
            if(string.IsNullOrEmpty(inputString))
            {
                return null;
            }
            
            return string.Join(
                ",",
                inputString.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x))
            );
        }
    }
}