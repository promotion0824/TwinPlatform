## Willow Notifications-Notification Service Library
## Overview
Willow Notifications library designed to enqueue notifications to a service bus queue. It simplifies the process of integrating notification services into Willow applications, providing a common model for notification handling.

## Getting Started
### Installation
Willow Notifications library is available as a NuGet package. You can install it using the NuGet Package Console window:
```
Install-Package Willow.Notifications
```
### Configuration
The following is an example of how to configure the Willow Notifications library in your application:
```
 services.AddNotificationsService(opt =>
            {
                opt.QueueName = "QueueName";
                opt.ServiceBusConnectionString = "ServiceBusConnectionString";
            });
```
The QueueName and ServiceBusConnectionString are required parameters.

### Usage
The following is an example of how to use the Willow Notifications library in your application:
```
public class NotificationService : INotificationService
	{
		private readonly INotificationService _notificationService;

		public NotificationService(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public async Task SendNotificationAsync(Notification notification)
		{
			await _notificationService.SendNotificationAsync(notification);
		}
	}
```
This example shows how to inject the INotificationService into your application and use it to send a notification to the service bus queue.
This notification will be enqueued immediately.

To schedule a notification to be enqueued at a later time, use the following method:
```
public class NotificationService : INotificationService
	{
		private readonly INotificationService _notificationService;

		public NotificationService(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public async Task SendNotificationAsync(Notification notification)
		{
			await _notificationService.SendScheduledNotificationAsync(notification, DateTime.UtcNow.AddMinutes(5));
		}
	}
```
