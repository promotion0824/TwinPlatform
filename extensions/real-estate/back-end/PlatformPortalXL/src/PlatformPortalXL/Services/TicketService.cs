using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Workflow;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Platform.Users;
using Willow.Workflow;
using Willow.Logging;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.ExceptionHandling.Exceptions;
using Willow.Workflow.Models;
using PlatformPortalXL.Features.Controllers;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Workflow.Requests;

namespace PlatformPortalXL.Services
{
	public interface ITicketService
	{
		Task<Ticket> CreateTicket(Guid siteId, WorkflowCreateTicketRequest request,
			IFormFileCollection attachmentFiles);

		Task<IList<TicketSimpleDto>> GetTicketList(Guid siteId, TicketListTab tab, bool scheduled, string orderBy,
			int page, int pageSize);

		Task<int> GetTotalTicketsCount(Guid siteId, TicketListTab tab, bool scheduled);
		Task<int[]> GetTicketsStatuses(TicketListTab tab, Guid siteId);
		Task EnrichTicket(Ticket ticket, Guid siteId);

        Task<List<TwinTicketStatisticsResponseDto>> GetTwinTicketStatisticsAsync(TicketTwinStatisticsRequest request);
        Task<List<TwinTicketStatisticsByStatus>> GetTwinTicketStatisticsByStatusAsync(TicketTwinStatisticsRequest request);
    }

	public class TicketService : ITicketService
	{
		private readonly IWorkflowApiService _workflowApi;
        private readonly ILogger<TicketService> _logger;
		private readonly IUserService _userService;
		private readonly ISiteApiService _siteApi;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly IDirectoryApiService _directoryApiService;

		public TicketService(IWorkflowApiService workflowApi,
            IUserService userService,
			ILogger<TicketService> logger,
			ISiteApiService siteApi,
            IDirectoryApiService directoryApiService,
            IDigitalTwinApiService digitalTwinApiService)
		{
			_workflowApi = workflowApi;
            _userService = userService;
			_logger = logger;
			_siteApi = siteApi;
            _directoryApiService = directoryApiService;
            _digitalTwinApiService = digitalTwinApiService;
		}

		public async Task<Ticket> CreateTicket(Guid siteId, WorkflowCreateTicketRequest request,
			IFormFileCollection attachmentFiles)
		{
			var createdTicket = await _workflowApi.CreateTicket(siteId, request);

			if (attachmentFiles != null && attachmentFiles.Any())
			{
				foreach (var attachmentFile in attachmentFiles)
				{
					var attachmentDto = new CreateStreamAttachmentDto
					{
						SiteId = siteId,
						TicketId = createdTicket.Id,
						FileName = attachmentFile.FileName,
						FileStream = attachmentFile.OpenReadStream(),
						SourceId = request.CreatorId
					};
					await _workflowApi.CreateAttachment(attachmentDto);
				}

				createdTicket = await _workflowApi.GetTicket(siteId, createdTicket.Id, true);
			}

			await EnrichTicket(createdTicket, siteId);

            return createdTicket;
		}

		public Task EnrichTicket(Ticket ticket, Guid siteId)
		{
			return Task.WhenAll
			(
                EnrichAssignee(ticket, siteId),
				EnrichCreator(ticket),
				EnrichComments(ticket, siteId)
			);
		}


		public class InvalidTabException : Exception
		{
			public InvalidTabException(TicketListTab tab) : base($"Unknown tab: {tab}")
			{
			}
		}

		public async Task<IList<TicketSimpleDto>> GetTicketList(Guid siteId, TicketListTab tab, bool scheduled,
			string orderBy, int page, int pageSize)
		{
			var statuses = await GetTicketsStatuses(tab, siteId);
			var tickets = await _workflowApi.GetTickets(siteId, statuses, null, null, null, scheduled,null, orderBy, page,
				pageSize);
            return TicketSimpleDto.MapFromModels(tickets);
		}

		public async Task<int> GetTotalTicketsCount(Guid siteId, TicketListTab tab, bool scheduled)
		{
			var statuses = await GetTicketsStatuses(tab, siteId);
			return await _workflowApi.GetTotalTicketsCount(siteId, statuses, scheduled);
		}

		public async Task<int[]> GetTicketsStatuses(TicketListTab tab, Guid siteId)
		{
			var site = await _siteApi.GetSite(siteId);
			var ticketStatuses = await _workflowApi.GetCustomerTicketStatus(site.CustomerId);

			int[] statuses;
			if (ticketStatuses?.Any() ?? false)
			{
				var statusQuery = ticketStatuses.AsQueryable();
				if (tab != TicketListTab.All)
				{
					statusQuery = statusQuery.Where(x =>
						x.Tab.Equals(tab.ToString(), StringComparison.InvariantCultureIgnoreCase));
				}

				statuses = statusQuery.Select(x => x.StatusCode).ToArray();
				if (!statuses.Any())
				{
					throw new InvalidTabException(tab);
				}
			}
			else
			{
				statuses = tab switch
				{
					TicketListTab.All => new[]
					{
						(int)TicketStatus.Open, (int)TicketStatus.Reassign, (int)TicketStatus.InProgress,
						(int)TicketStatus.LimitedAvailability, (int)TicketStatus.Resolved, (int)TicketStatus.Closed
					},
					TicketListTab.Open => new[]
					{
						(int)TicketStatus.Open, (int)TicketStatus.Reassign, (int)TicketStatus.InProgress,
						(int)TicketStatus.LimitedAvailability
					},
					TicketListTab.Resolved => new[] { (int)TicketStatus.Resolved },
					TicketListTab.Closed => new[] { (int)TicketStatus.Closed },
					_ => throw new InvalidTabException(tab)
				};
			}

			return statuses;
		}

        public async Task<List<TwinTicketStatisticsResponseDto>> GetTwinTicketStatisticsAsync(TicketTwinStatisticsRequest request)
        {
            var twins = await _digitalTwinApiService.GetTwinsWithGeometryIdAsync(request);

            if (twins?.Any() ?? false)
            {
                var twinStats = await _workflowApi.GetTwinsTicketStatistics(new TwinTicketStatisticApiRequest{ TwinIds = twins.Select(c => c.TwinId).ToList(),SourceTypes = request.SourceTypes});

                var result = new List<TwinTicketStatisticsResponseDto>();
                foreach (var twin in twins)
                {
                    var twinTicketStatsResponse = new TwinTicketStatisticsResponseDto
                    {
                        TwinId = twin.TwinId,
                        GeometryViewerId = twin.GeometryViewerId,
                        UniqueId = twin.UniqueId
                    };

                    var twinTicketStats = twinStats.FirstOrDefault(c => c.TwinId == twin.TwinId);
                    if (twinTicketStats != null)
                    {
                        twinTicketStatsResponse.HighestPriority = twinTicketStats.HighestPriority;
                        twinTicketStatsResponse.TicketCount = twinTicketStats.TicketCount;
                    }

                    result.Add(twinTicketStatsResponse);
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Returns ticket status statistics by twinIds
        /// </summary>
        /// <param name="request">Twins and source types</param>
        /// <returns>Ticket status statistics by twinIds</returns>
        public async Task<List<TwinTicketStatisticsByStatus>> GetTwinTicketStatisticsByStatusAsync(TicketTwinStatisticsRequest request)
        {
            return await _workflowApi.GetTwinsTicketStatisticsByStatus(new TwinTicketStatisticApiRequest
            {
                TwinIds = request.DtIds,
                SourceTypes = request.SourceTypes
            });
        }

        #region Private

        private async Task EnrichCreator(Ticket ticket)
		{
			try
			{
				var user = await _directoryApiService.GetUser(ticket.CreatorId);

				ticket.Creator = user.ToCreator();

				return;
			}
			catch (NotFoundException nfEx)
			{
				_logger.LogWarning("Ticket creator not found", nfEx,
					new { SiteId = ticket.SiteId, TicketId = ticket.Id, creatorId = ticket.CreatorId });
			}
			catch (Exception ex)
			{
				_logger.LogError("Unable to set the ticket creator", ex,
					new { SiteId = ticket.SiteId, TicketId = ticket.Id, creatorId = ticket.CreatorId });
			}
		}

		private async Task EnrichAssignee(Ticket ticket, Guid siteId)
		{
			if (ticket.AssigneeType != TicketAssigneeType.NoAssignee)
			{
				if (ticket.AssigneeId.HasValue)
				{
					try
					{
						var userType = ticket.AssigneeType switch
						{
							TicketAssigneeType.CustomerUser => UserType.Customer,
							TicketAssigneeType.WorkGroup => UserType.Workgroup,
							_ => UserType.All
						};
						var user = await _userService.GetUser(siteId, ticket.AssigneeId.Value, userType);

						ticket.Assignee = user.ToAssignee();

						return;
					}
					catch (NotFoundException nfEx)
					{
						_logger.LogWarning("Ticket assignee not found", nfEx,
							new
							{
								SiteId = siteId, TicketId = ticket.Id, AssigneeId = ticket.AssigneeId,
								AssigneeType = ticket.AssigneeType
							});
					}
					catch (Exception ex)
					{
						_logger.LogError("Unable to set the ticket assignee", ex,
							new
							{
								SiteId = siteId, TicketId = ticket.Id, AssigneeId = ticket.AssigneeId,
								AssigneeType = ticket.AssigneeType
							});
					}
				}
                // the ticket assignee user can be in external profiles in workflow core
				ticket.Assignee = new TicketAssignee
				{
					Type = ticket.AssigneeType,
					Id = ticket.AssigneeId ?? Guid.Empty,
					FirstName = string.IsNullOrWhiteSpace(ticket.AssigneeName)? "Unknown" : ticket.AssigneeName,
					Name = string.IsNullOrWhiteSpace(ticket.AssigneeName) ? "Unknown" : ticket.AssigneeName,
                };
			}
		}

		private async Task EnrichComments(Ticket ticket, Guid siteId)
		{
			if (ticket.Comments != null && ticket.Comments.Count > 0)
			{
				ticket.Comments = ticket.Comments.OrderByDescending(x => x.CreatedDate).ToList();

				try
				{
					var creatorIds = ticket.Comments.Where(c => c.CreatorId != Guid.Empty)
						.Select(c => c.CommentUserId()).Distinct();
					var creators = (await _userService.GetUsers(siteId, creatorIds, UserType.All))
						.Distinct(new UserComparer()).ToDictionary(k => k.Id, v => v);

					foreach (var comment in ticket.Comments)
					{
						if (creators.TryGetValue(comment.CreatorId, out IUser creator))
						{
							comment.Creator = new CommentCreator
							{
								Type = creator.Type switch
								{
									UserType.Customer => CommentCreatorType.CustomerUser,
									_ => CommentCreatorType.CustomerUser
								},
								Id = creator.Id,
								FirstName = creator.FirstName,
								LastName = creator.LastName,
								Email = creator.Email
							};
						}
						else
						{
							comment.Creator = new CommentCreator
							{
								Type = CommentCreatorType.CustomerUser,
								Id = comment.CreatorId,
								FirstName = "Unknown"
							};
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Unable to set the comment creators");
				}
			}
		}

		#endregion
	}
}
