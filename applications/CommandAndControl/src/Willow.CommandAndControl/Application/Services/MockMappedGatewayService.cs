namespace Willow.CommandAndControl.Application.Services;

internal class MockMappedGatewayService : IMappedGatewayService
{
    public async Task<SetValueResponse> SendSetValueCommandAsync(string pointId, double value)
    {
        await Task.Delay(10000);
        var interval = DateTime.UtcNow.Second / 10;

        //For a 10 second interval it will return success and for rest it will return failure
        if (interval == 0 || interval % 2 == 0)
        {
            return new SetValueResponse()
            {
                StatusCode = HttpStatusCode.OK,
            };
        }

        var errorResultType = new[] { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound };
        var random = new Random();

        return new SetValueResponse()
        {
            StatusCode = errorResultType[random.Next(0, errorResultType.Length)],
            Error = new Error()
            {
                Description = "Error while setting value",
            },
        };
    }
}
