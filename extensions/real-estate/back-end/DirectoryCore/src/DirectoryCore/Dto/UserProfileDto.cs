using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Dto;

public class UserProfileDto
{
    /// <summary>
    /// User Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User First Name
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// User Last Name
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// User Email
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// User Phone
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// User Company
    /// </summary>
    public string Company { get; set; }

    public static UserProfileDto MapFrom(User user)
    {
        if (user == null)
        {
            return null;
        }

        var userProfileDto = new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Mobile,
            Company = user.Company
        };

        return userProfileDto;
    }

    public static List<UserProfileDto> MapFrom(List<User> users)
    {
        if (users == null)
        {
            return null;
        }

        var userProfileDtos = users.Select(MapFrom).ToList();

        return userProfileDtos;
    }
}
