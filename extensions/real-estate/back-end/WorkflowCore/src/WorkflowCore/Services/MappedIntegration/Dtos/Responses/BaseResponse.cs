using System.Collections.Generic;

namespace WorkflowCore.Services.MappedIntegration.Dtos.Responses;

public class BaseResponse
{
    /// <summary>
    /// status of the response
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// validation error list
    /// </summary>
    public List<string> ErrorList { get; set; }

    public static BaseResponse CreateSuccess()
    {
        return new BaseResponse
        {
            IsSuccess = true
        };
    }

    public static BaseResponse CreateFailure(List<string> errorList)
    {
        return new BaseResponse
        {
            IsSuccess = false,
            ErrorList = errorList
        };
    }

    public static BaseResponse CreateFailure(string errorMessage)
    {
        var errorList = new List<string> { errorMessage };
        return new BaseResponse
        {
            IsSuccess = false,
            ErrorList = errorList
        };
    }
}
