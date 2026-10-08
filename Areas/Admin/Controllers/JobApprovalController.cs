using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Core.Application.UseCases.Jobs;

namespace ITInternHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public class JobApprovalController : Controller
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly ApproveJobUseCase _approveJobUseCase;
        private readonly RejectJobUseCase _rejectJobUseCase;
        private readonly BulkApproveJobsUseCase _bulkApproveUseCase;
        private readonly BulkRejectJobsUseCase _bulkRejectUseCase;

        public JobApprovalController(
            IJobPostRepository jobRepository,
            ApproveJobUseCase approveJobUseCase,
            RejectJobUseCase rejectJobUseCase,
            BulkApproveJobsUseCase bulkApproveUseCase,
            BulkRejectJobsUseCase bulkRejectUseCase)
        {
            _jobRepository = jobRepository;
            _approveJobUseCase = approveJobUseCase;
            _rejectJobUseCase = rejectJobUseCase;
            _bulkApproveUseCase = bulkApproveUseCase;
            _bulkRejectUseCase = bulkRejectUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pendingJobs = await _jobRepository.GetPendingJobsAsync();
            var publishedJobs = await _jobRepository.GetPublishedJobsAsync();

            var viewModel = new ITInternHub.Models.ViewModels.JobApprovalIndexViewModel
            {
                PendingJobs = pendingJobs,
                PublishedJobs = publishedJobs
            };

            return View(viewModel);
        }

        // AJAX: Duyệt đơn lẻ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(long id)
        {
            try
            {
                long adminUserId = GetCurrentUserId();
                var (success, message) = await _approveJobUseCase.ExecuteAsync(id, adminUserId);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Từ chối đơn lẻ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(long id, string reason)
        {
            try
            {
                long adminUserId = GetCurrentUserId();
                var (success, message) = await _rejectJobUseCase.ExecuteAsync(id, reason, adminUserId);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Duyệt hàng loạt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkApprove([FromForm] List<long> selectedJobIds)
        {
            try
            {
                long adminUserId = GetCurrentUserId();
                var (success, message, count) = await _bulkApproveUseCase.ExecuteAsync(selectedJobIds, adminUserId);
                return Json(new { success, message, count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Từ chối hàng loạt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkReject([FromForm] List<long> selectedJobIds, string bulkRejectReason)
        {
            try
            {
                long adminUserId = GetCurrentUserId();
                var (success, message, count) = await _bulkRejectUseCase.ExecuteAsync(selectedJobIds, bulkRejectReason, adminUserId);
                return Json(new { success, message, count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private long GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idClaim, out var val) ? val : 1;
        }
    }
}