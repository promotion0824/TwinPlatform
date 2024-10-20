using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Dto;

public record FullNameDto(Guid UserId, string FirstName, string LastName)
{
    public static FullNameDto MapFromUser(User user)
    {
        return new FullNameDto(user.Id, user.FirstName, user.LastName);
    }

    public static List<FullNameDto> MapFromUsers(List<User> users)
    {
        return users.Select(MapFromUser).ToList();
    }
}
