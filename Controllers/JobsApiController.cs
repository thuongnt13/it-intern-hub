using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITInternHub.Models;

namespace ITInternHub.Controllers
{
    [ApiController]
    [Route("api/v1/jobs")]
    public class JobsApiController : ControllerBase
    {
        private readonly ITInternHubDbContext _context;

        public JobsApiController(ITInternHubDbContext context)
        {
            _context = context;
        }

        // GET: http://localhost:5068/api/v1/jobs?keyword=React&industryId=1&city=Hà+Nội
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? keyword,
            [FromQuery] int? industryId,
            [FromQuery] string? city,
            [FromQuery] string? skill,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            var today = DateOnly.FromDateTime(DateTime.Today);

            var query = _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.Industry)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.Status == 2 && j.Deadline >= today)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) || j.Company.CompanyName.Contains(keyword));
            }
            if (industryId.HasValue && industryId.Value > 0)
            {
                query = query.Where(j => j.IndustryId == industryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(j => j.City == city);
            }
            if (!string.IsNullOrWhiteSpace(skill))
            {
                query = query.Where(j => j.JobSkills.Any(js => js.Skill.Name == skill));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    id = j.Id,
                    title = j.Title,
                    company_name = j.Company.CompanyName,
                    city = j.City,
                    salary_min = j.SalaryMin,
                    salary_max = j.SalaryMax,
                    deadline = j.Deadline.ToString("yyyy-MM-dd"),
                    required_skills = j.JobSkills.Where(js => js.PriorityLevel == 1).Select(js => js.Skill.Name).ToList(),
                    preferred_skills = j.JobSkills.Where(js => js.PriorityLevel == 2).Select(js => js.Skill.Name).ToList()
                })
                .ToListAsync();

            // Trả về JSON chuẩn RESTful
            return Ok(new
            {
                page = page,
                page_size = pageSize,
                total_items = totalItems,
                total_pages = totalPages,
                data = jobs
            });
        }
    }
}