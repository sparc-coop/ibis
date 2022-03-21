using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features
{
    public record ProjectRequest(string projectId, string userId);
    public class GetOrCreateProject : PublicFeature<ProjectRequest, GetProjectResponse>
    {
        public GetOrCreateProject(IRepository<Project> projects)
        {
            Projects = projects;
        }

        public IRepository<Project> Projects { get; }

        public override async Task<GetProjectResponse> ExecuteAsync(ProjectRequest request)
        {
            Project project = new Project();

            if(request.projectId == null)
            {
                await Projects.AddAsync(project);
            }
            else
            {
                project = await Projects.FindAsync(id: request.projectId);
            }

            return new(project.Id, project.Name, project.Url, project.UserId);
        }
    }
}
