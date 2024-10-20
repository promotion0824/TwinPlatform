using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Entities;

namespace WsupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ILogger<TeamsController> _logger;
        private readonly WsupDbContext wsupDbContext;

        public TeamsController(ILogger<TeamsController> logger, WsupDbContext wsupDbContext)
        {
            _logger = logger;
            this.wsupDbContext = wsupDbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Team> GetAll()
        {
            return wsupDbContext.Teams;
        }

        [HttpGet("Get")]
        public Team? Get(int teamId)
        {
            return wsupDbContext.Teams.FirstOrDefault(s => s.Id == teamId);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Team> Create(Team team)
        {
            await wsupDbContext.Teams.AddAsync(team);
            await wsupDbContext.SaveChangesAsync();
            return team;
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task<Team> Update(Team team)
        {
            wsupDbContext.Teams.Update(team);
            await wsupDbContext.SaveChangesAsync();
            return team;
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "WsupApi-Writer")]
        public async Task Delete(int teamId)
        {
            var team = wsupDbContext.Teams.FirstOrDefault(s => s.Id == teamId);

            if (team != null)
            {
                wsupDbContext.Teams.Remove(team);
                await wsupDbContext.SaveChangesAsync();
            }
        }
    }
}
