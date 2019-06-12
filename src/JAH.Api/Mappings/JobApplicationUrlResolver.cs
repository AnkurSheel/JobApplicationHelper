using AutoMapper;

using JAH.Api.Controllers;
using JAH.Data.Entities;
using JAH.DomainModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Mappings
{
    public class JobApplicationUrlResolver : IValueResolver<JobApplicationEntity, JobApplication, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobApplicationUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string Resolve(JobApplicationEntity source, JobApplication destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.Urlhelper];
            return url.Link("GetJobApplication", new { companyName = source.CompanyName });
        }
    }
}
