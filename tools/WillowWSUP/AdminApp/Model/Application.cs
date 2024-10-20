namespace Willow.AdminApp;

public record Application(string Name, int CountHealthy, int CountDegraded, int CountUnhealthy, string[] Versions);
