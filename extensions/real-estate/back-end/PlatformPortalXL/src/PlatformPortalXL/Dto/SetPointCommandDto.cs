using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class SetPointCommandDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public Guid ConnectorId { get; set; }
        public Guid EquipmentId { get; set; }
        public Guid InsightId { get; set; }
        public Guid PointId { get; set; }
        public Guid SetPointId { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal OriginalValue { get; set; }
        public decimal DesiredValue { get; set; }
        public int DesiredDurationMinutes { get; set; }
        public SetPointCommandStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string ErrorDescription { get; set; }
        public string Unit { get; set; }
        public SetPointCommandType Type { get; set; }
        public UserSimpleDto CreatedBy { get; set; }

        public async static Task<SetPointCommandDto> MapFromAsync(SetPointCommand model, IUserService userService) => 
            new SetPointCommandDto
            {
                Id = model.Id,
                ConnectorId = model.ConnectorId,
                CreatedAt = model.CreatedAt,
                CurrentReading = model.CurrentReading,
                DesiredDurationMinutes = model.DesiredDurationMinutes,
                DesiredValue = model.DesiredValue,
                ErrorDescription = model.ErrorDescription,
                EquipmentId = model.EquipmentId,
                InsightId = model.InsightId,
                LastUpdatedAt = model.LastUpdatedAt,
                OriginalValue = model.OriginalValue,
                PointId = model.PointId,
                SetPointId = model.SetPointId,
                SiteId = model.SiteId,
                Status = model.Status,
                Type = model.Type,
                Unit = model.Unit,
                CreatedBy = await LookupUserAsync(userService, model)
            };

        private static async Task<UserSimpleDto> LookupUserAsync(IUserService userService, SetPointCommand model)
        {
            if (model.CreatedBy == null)
            {
                return null;
            }

            try
            {
                var user = await userService.GetUser(model.SiteId, model.CreatedBy.Value);
    
                return UserSimpleDto.Map(user);
            }
            catch
            {

            }

            return new UserSimpleDto
            {
                Id = model.CreatedBy.Value,
            };
        }

        public static async Task<IList<SetPointCommandDto>> MapFrom(IEnumerable<SetPointCommand> models, IUserService userService)
        {
            var output = new List<SetPointCommandDto>();
            foreach (var model in models)
            {
                output.Add(await MapFromAsync(model, userService));
            }

            return output;
        }

        internal SetPointCommand MapToModel()
        {
            return new SetPointCommand
            {
                Id = Id,
                ConnectorId = ConnectorId,
                CreatedAt = CreatedAt,
                DesiredDurationMinutes = DesiredDurationMinutes,
                CurrentReading = CurrentReading,
                DesiredValue = DesiredValue,
                ErrorDescription = ErrorDescription,
                EquipmentId = EquipmentId,
                InsightId = InsightId,
                LastUpdatedAt = LastUpdatedAt,
                OriginalValue = OriginalValue,
                PointId = PointId,
                SetPointId = SetPointId,
                SiteId = SiteId,
                Status = Status,
                Type = Type,
                Unit = Unit
            };
        }
    }
}
