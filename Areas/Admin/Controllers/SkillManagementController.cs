using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Core.Application.DTOs;
using ITInternHub.Core.Application.UseCases.Skills;

namespace ITInternHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public class SkillManagementController : Controller
    {
        private readonly ISkillRepository _skillRepository;
        private readonly CreateSkillUseCase _createSkillUseCase;
        private readonly UpdateSkillUseCase _updateSkillUseCase;
        private readonly DeleteSkillUseCase _deleteSkillUseCase;
        private readonly ToggleSkillHotUseCase _toggleHotUseCase;

        public SkillManagementController(
            ISkillRepository skillRepository,
            CreateSkillUseCase createSkillUseCase,
            UpdateSkillUseCase updateSkillUseCase,
            DeleteSkillUseCase deleteSkillUseCase,
            ToggleSkillHotUseCase toggleHotUseCase)
        {
            _skillRepository = skillRepository;
            _createSkillUseCase = createSkillUseCase;
            _updateSkillUseCase = updateSkillUseCase;
            _deleteSkillUseCase = deleteSkillUseCase;
            _toggleHotUseCase = toggleHotUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? industryId, string? keyword)
        {
            ViewBag.Industries = await _skillRepository.GetAllIndustriesAsync();
            ViewBag.CurrentIndustryId = industryId;
            ViewBag.CurrentKeyword = keyword;

            var skills = await _skillRepository.GetAllSkillsAsync(industryId, keyword);
            return View(skills);
        }

        // AJAX: Thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSkillDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu nhập không hợp lệ!" });

                long adminUserId = GetCurrentUserId();
                var (success, message) = await _createSkillUseCase.ExecuteAsync(dto, adminUserId);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateSkillDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu chỉnh sửa không hợp lệ!" });

                long adminUserId = GetCurrentUserId();
                var (success, message) = await _updateSkillUseCase.ExecuteAsync(dto, adminUserId);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Đổi trạng thái HOT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHot(int id)
        {
            try
            {
                var (success, message) = await _toggleHotUseCase.ExecuteAsync(id);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Xóa an toàn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                long adminUserId = GetCurrentUserId();
                var (success, message) = await _deleteSkillUseCase.ExecuteAsync(id, adminUserId);
                return Json(new { success, message });
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