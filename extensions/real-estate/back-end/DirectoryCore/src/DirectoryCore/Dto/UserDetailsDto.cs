using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services;

namespace DirectoryCore.Dto;

public class UserDetailsDto
{
    public UserDetailsDto()
    {
        UserAssignments = Enumerable.Empty<UserAssignment>().ToList();
    }

    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Initials { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public UserStatus Status { get; set; }
    public string Auth0UserId { get; set; }

    public string Name => ((FirstName ?? "") + " " + (LastName ?? "")).Trim();

    public string Company { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerDto Customer { get; set; }

    public List<UserAssignment> UserAssignments { get; set; }

    public static UserDetailsDto MapFrom(
        UserEntity userEntity,
        CustomerEntity customerEntity,
        IImagePathHelper imagePathHelper
    )
    {
        if (userEntity is null || customerEntity is null)
        {
            return null;
        }
        else
        {
            return new UserDetailsDto
            {
                Id = userEntity.Id,
                FirstName = userEntity.FirstName,
                LastName = userEntity.LastName,
                Email = userEntity.Email,
                Mobile = userEntity.Mobile,
                Company = userEntity.Company,
                Initials = userEntity.Initials,
                CreatedDate = userEntity.CreatedDate,
                Status = userEntity.Status,
                Auth0UserId = userEntity.Auth0UserId,
                CustomerId = userEntity.CustomerId,
                Customer = CustomerDto.MapFrom(
                    CustomerEntity.MapTo(customerEntity),
                    imagePathHelper
                )
            };
        }
    }
}

public record UserAssignment(string permissionId, Guid resourceId);
