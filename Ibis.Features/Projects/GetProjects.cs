using Sparc.Core;
using Sparc.Features;


namespace Ibis.Features
{
    public record GetProjectResponse(string ProjectId, string ProjectName, string ProjectUrl, string UserId);
    public class GetProjects : PublicFeature<string, List<GetProjectResponse>>
    {
        public IRepository<Project> Projects{ get; }
        public GetProjects(IRepository<Project> projects)
        {
            Projects = projects;
        }

        public override async Task<List<GetProjectResponse>> ExecuteAsync(string userId)
        {
            List<Project> projectList = await Projects.Query.Where(x => x.UserID == userId).ToListAsync();
            if (projectList == null)
                throw new NotFoundException($"No projects found!");

            return new List<GetProjectResponse>();
        }
    }
}
