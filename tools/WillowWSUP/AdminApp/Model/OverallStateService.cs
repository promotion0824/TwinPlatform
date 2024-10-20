namespace Willow.AdminApp;

public class OverallStateService
{
    public OverallState State { get; }

    public OverallStateService()
    {
        this.State = new OverallState();
    }
}
