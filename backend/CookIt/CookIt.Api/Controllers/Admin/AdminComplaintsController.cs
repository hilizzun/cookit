using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CookIt.API.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Moderator")]
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly ILogger<ComplaintsController> _logger;

        public ComplaintsController(IComplaintService complaintService, ILogger<ComplaintsController> logger)
        {
            _complaintService = complaintService;
            _logger = logger;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var complaints = await _complaintService.GetPendingComplaintsAsync();
            return Ok(complaints);
        }

        [HttpPost("{complaintId}/resolve")]
        public async Task<IActionResult> Resolve(int complaintId, [FromBody] ResolveComplaintDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            try
            {
                await _complaintService.ResolveComplaintAsync(complaintId, adminId, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке жалобы");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}