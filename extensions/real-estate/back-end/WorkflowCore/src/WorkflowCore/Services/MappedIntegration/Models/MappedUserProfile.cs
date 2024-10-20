using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Entities;
using WorkflowCore.Models;

namespace WorkflowCore.Services.MappedIntegration.Models;

public class MappedUserProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Company { get; set; }

    public static MappedUserProfile MapFromUserProfile(UserProfile userProfile)
    {
        if (userProfile is null)
        {
            return null;
        }
        return new MappedUserProfile
        {
            Id = userProfile.Id,
            Name = $"{userProfile.FirstName} {userProfile.LastName}",
            Email = userProfile.Email,
            Phone = userProfile.Phone,
            Company = userProfile.Company
        };
    }
    public static List<MappedUserProfile> MapFromUserProfiles(List<UserProfile> userProfiles)
    {
        return userProfiles.Select(MapFromUserProfile).ToList();
    }

    public static MappedUserProfile MapFrom(ExternalProfileEntity externalProfileEntity)
    {
        if (externalProfileEntity is null)
        {
            return null;
        }

        return new MappedUserProfile
        {
            Id = externalProfileEntity.Id,
            Name = externalProfileEntity.Name,
            Email = externalProfileEntity.Email,
            Phone = externalProfileEntity.Phone,
            Company = externalProfileEntity.Company
        };
    }

    public static List<MappedUserProfile> MapFrom(List<ExternalProfileEntity> externalProfileEntities)
    {
        if(externalProfileEntities is null)
        {
            return new List<MappedUserProfile>();
        }

        return externalProfileEntities.Select(MapFrom).ToList();
    }
}

