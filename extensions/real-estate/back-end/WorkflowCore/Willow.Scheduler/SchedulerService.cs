using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Willow.Calendar;
using Willow.ExceptionHandling.Exceptions;

namespace Willow.Scheduler
{
	public class SchedulerService : ISchedulerService
	{
		private readonly ISchedulerRepository _repo;
		private readonly ILogger<SchedulerService> _logger;
		private readonly int _advance;
		private readonly IDictionary<string, IScheduleRecipient> _recipients;

		public SchedulerService(ISchedulerRepository repo,
								ILogger<SchedulerService> logger,
								IDictionary<string, IScheduleRecipient> recipients,
								int advance)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(recipients));
			_logger = logger;
			_recipients = recipients ?? throw new ArgumentNullException(nameof(recipients));
			_advance = advance;
		}

		#region ISchedulerService

		public async Task CheckSchedules(DateTime dtNow, string language)
		{
			var dtCheck = dtNow.AddDays(_advance);
			var hits = this.GetMatching(dtCheck);
			var numHits = 0;

			await foreach (var hit in hits)
			{
				++numHits;

				await PerformScheduleHit(hit.Schedule.RecipientClient, hit.Schedule.Recipient, new ScheduleHit
				{
					ScheduleId = hit.Schedule.Id,
					OwnerId = hit.Schedule.OwnerId,
					HitDate = hit.SiteDateTime,
					EventName = hit.Event.Name,
					Recurrence = hit.Event.Occurs
				}, language);
			}

			var dtLog = dtCheck.ToString("s");

			_logger.LogInformation($"{numHits} matching schedule hits found for {dtLog}");

			return;
		}

		public async Task<List<ScheduleHit>> GetSchedulesByOwnerId(DateTime utcNow, IList<Guid> ownerIds)
		{
			var dtCheck = utcNow.AddDays(_advance);
			var schedules = await GetMatchingByOwnerIds(ownerIds, dtCheck);

			return schedules.Select(s => new ScheduleHit
			{
				ScheduleId = s.Schedule.Id,
				OwnerId = s.Schedule.OwnerId,
				HitDate = s.SiteDateTime,
				EventName = s.Event.Name

			}).ToList();
		}

		public async IAsyncEnumerable<(Schedule Schedule, Event Event, DateTime SiteDateTime)> GetMatching(DateTime dtCheck)
		{
			var schedules = await _repo.GetSchedules();

			foreach (var schedule in schedules)
			{
				if (ScheduleMatches(schedule, dtCheck, out Event evt))
					yield return (schedule, evt, dtCheck.InTimeZone(evt.Timezone));
			}
		}

		public async Task<IEnumerable<(Schedule Schedule, Event Event, DateTime SiteDateTime)>> GetMatchingByOwnerIds(IList<Guid> ownerIds, DateTime dtCheck)
		{
			var schedules = await _repo.GetSchedulesByOwnerId(ownerIds);
			var list = new List<(Schedule Schedule, Event Event, DateTime SiteDateTime)>();

			foreach (var schedule in schedules)
			{
				if (ScheduleMatches(schedule, dtCheck, out Event evt))
					list.Add((schedule, evt, dtCheck.InTimeZone(evt.Timezone)));
			}

			return list;
		}

		#endregion

		#region Private

		private bool ScheduleMatches(Schedule schedule, DateTime dtCheck, out Event evtOut)
		{
			evtOut = null;

			try
			{
				if (schedule.Active)
				{
					var evt = JsonConvert.DeserializeObject<Event>(schedule.Recurrence);

					if (string.IsNullOrWhiteSpace(evt.Timezone))
					{
						_logger.LogError($"Schedule is missing timezone: {schedule.Id}");
						return false;
					}

					var dtSite = dtCheck.InTimeZone(evt.Timezone);

					if (evt.Matches(dtSite))
					{
						evtOut = evt;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error in evaluating schedule: {schedule.Id}");
			}

			return false;
		}

		private async Task PerformScheduleHit(string recipientClient, string recipient, ScheduleHit scheduleHit, string language)
		{
			if (recipientClient != "ServiceBus")
			{
				try
				{
					if (!_recipients.ContainsKey($"{recipientClient}:{recipient}"))
					{
						var msg = $"Unknown recipient: {recipientClient},{recipient}";

						_logger.LogError(new Exception(msg), msg);
						return;
					}

					await _recipients[$"{recipientClient}:{recipient}"].PerformScheduleHit(scheduleHit, language);

					_logger.LogInformation($"Schedule hit for recipient {recipientClient}:{recipient} sent");

				}
				catch (NotFoundException nfex) when (nfex.Message == "TicketTemplate not found")
				{
					try
					{
						await _repo.DeleteSchedule(scheduleHit.ScheduleId);

						_logger.LogInformation($"Orphan schedule deleted: {scheduleHit.ScheduleId}");
					}
					catch (Exception ex2)
					{
						_logger.LogError(ex2, $"Unable to delete orphan schedule: {scheduleHit.ScheduleId}");
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Unable to call schedule hit recipient");
				}
			}
			else
				_logger.LogError($"Unsupported schedule hit type: [{recipientClient}] for schedule: {scheduleHit.ScheduleId}");

			return;
		}

		#endregion
	}
}
