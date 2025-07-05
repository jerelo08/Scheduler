using TAMHR.Hangfire.Models;

namespace TAMHR.Hangfire.Services
{
    public interface IModelMapper
    {
        UserPostDto MapToDto(User user);
        ActualOrgPostDto MapToDto(ActualOrganizationStructure actualOrg);
        ActualEntityPostDto MapToDto(ActualEntityStructure actualEntity);
        OrganizationObjectPostDto MapToDto(OrganizationObject orgObject);
        EventsCalendarPostDto MapToDto(EventsCalendar eventsCalendar);
        
        IEnumerable<UserPostDto> MapToDto(IEnumerable<User> users);
        IEnumerable<ActualOrgPostDto> MapToDto(IEnumerable<ActualOrganizationStructure> actualOrgs);
        IEnumerable<ActualEntityPostDto> MapToDto(IEnumerable<ActualEntityStructure> actualEntities);
        IEnumerable<OrganizationObjectPostDto> MapToDto(IEnumerable<OrganizationObject> orgObjects);
        IEnumerable<EventsCalendarPostDto> MapToDto(IEnumerable<EventsCalendar> eventsCalendars);
    }

    public class ModelMapper : IModelMapper
    {
        public UserPostDto MapToDto(User user)
        {
            var now = DateTime.Now;
            return new UserPostDto
            {
                ImportType = 1,
                ImportStatus_ID = 1,
                BatchTag = "Tag",
                ErrorCode = 200,
                Code = user.Id.ToString(),
                Name = user.Name,
                NewCode = user.Username,
                UniqueCode = user.NoReg,
                created_at_system = now.ToString("s"),
                ID = "",
                NoReg = user.NoReg,
                Username = user.Username,
                Email = user.Email ?? "",
                CreatedOn = now.ToString("yyyy-MM-dd"),
                CreatedBy = "System",
                LastLogin = "",
                IsActive = user.IsActive == 1 ? "1" : "0"
            };
        }

        public ActualOrgPostDto MapToDto(ActualOrganizationStructure actualOrg)
        {
            var now = DateTime.Now;
            return new ActualOrgPostDto
            {
                Code = actualOrg.Id.ToString(),
                Name = actualOrg.OrgName,
                OrgCode = actualOrg.OrgCode,
                ParentOrgCode = actualOrg.ParentOrgCode,
                Service = actualOrg.Service,
                NoReg = actualOrg.NoReg,
                PostCode = actualOrg.PostCode,
                PostName = actualOrg.PostName,
                JobCode = actualOrg.JobCode,
                JobName = actualOrg.JobName,
                EmployeeGroup = actualOrg.EmployeeGroup,
                created_at_system = now.ToString("s"),
                ImportType = 1,
                ImportStatus_ID = 1,
                BatchTag = "Tag",
                ErrorCode = 200,
                ID = ""
            };
        }

        public ActualEntityPostDto MapToDto(ActualEntityStructure actualEntity)
        {
            var now = DateTime.Now;
            return new ActualEntityPostDto
            {
                Code = actualEntity.Id.ToString(),
                Name = actualEntity.ObjectText,
                OrgCode = actualEntity.OrgCode,
                ObjectCode = actualEntity.ObjectCode,
                ObjectText = actualEntity.ObjectText,
                ObjectDescription = actualEntity.ObjectDescription,
                created_at_system = now.ToString("s"),
                ImportType = 1,
                ImportStatus_ID = 1,
                BatchTag = "Tag",
                ErrorCode = 200,
                ID = ""
            };
        }

        public OrganizationObjectPostDto MapToDto(OrganizationObject orgObject)
        {
            var now = DateTime.Now;
            return new OrganizationObjectPostDto
            {
                Code = orgObject.Id.ToString(),
                ObjectType = orgObject.ObjectType,
                ObjectID = orgObject.ObjectID,
                Abbreviation = orgObject.Abbreviation,
                ObjectText = orgObject.ObjectText,
                StartDate = orgObject.StartDate.ToString("yyyy-MM-dd"),
                EndDate = orgObject.EndDate.ToString("yyyy-MM-dd"),
                ObjectDescription = orgObject.ObjectDescription,
                created_at_system = now.ToString("s"),
                ImportType = 1,
                ImportStatus_ID = 1,
                BatchTag = "Tag",
                ErrorCode = 200,
                ID = ""
            };
        }

        public EventsCalendarPostDto MapToDto(EventsCalendar eventsCalendar)
        {
            var now = DateTime.Now;
            return new EventsCalendarPostDto
            {
                Code = eventsCalendar.Id.ToString(),
                Name = eventsCalendar.Title,
                EventTypeCode = eventsCalendar.EventTypeCode,
                StartDate = eventsCalendar.StartDate.ToString("yyyy-MM-dd"),
                EndDate = eventsCalendar.EndDate.ToString("yyyy-MM-dd"),
                Description = eventsCalendar.Description,
                CreatedBy = eventsCalendar.CreatedBy,
                CreatedOn = eventsCalendar.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"),
                ModifiedBy = eventsCalendar.ModifiedBy,
                ModifiedOn = eventsCalendar.ModifiedOn?.ToString("yyyy-MM-dd HH:mm:ss"),
                RowStatus = eventsCalendar.RowStatus ? "1" : "0",
                created_at_system = now.ToString("s"),
                ImportType = 1,
                ImportStatus_ID = 1,
                BatchTag = "Tag",
                ErrorCode = 200,
                ID = ""
            };
        }

        // Collection mapping methods
        public IEnumerable<UserPostDto> MapToDto(IEnumerable<User> users)
        {
            return users.Select(MapToDto);
        }

        public IEnumerable<ActualOrgPostDto> MapToDto(IEnumerable<ActualOrganizationStructure> actualOrgs)
        {
            return actualOrgs.Select(MapToDto);
        }

        public IEnumerable<ActualEntityPostDto> MapToDto(IEnumerable<ActualEntityStructure> actualEntities)
        {
            return actualEntities.Select(MapToDto);
        }

        public IEnumerable<OrganizationObjectPostDto> MapToDto(IEnumerable<OrganizationObject> orgObjects)
        {
            return orgObjects.Select(MapToDto);
        }

        public IEnumerable<EventsCalendarPostDto> MapToDto(IEnumerable<EventsCalendar> eventsCalendars)
        {
            return eventsCalendars.Select(MapToDto);
        }
    }
}
