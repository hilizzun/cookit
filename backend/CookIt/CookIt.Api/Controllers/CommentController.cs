using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CookIt.API.Controllers
{
    [Route("api/recipes/{recipeId}/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;
        private readonly IComplaintService _complaintService;

        public CommentsController(
            ICommentService commentService,
            ILogger<CommentsController> logger,
            IComplaintService complaintService)
        {
            _commentService = commentService;
            _logger = logger;
            _complaintService = complaintService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int recipeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var comments = await _commentService.GetCommentsForRecipeAsync(recipeId, page, pageSize);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(int recipeId, [FromBody] CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var comment = await _commentService.AddCommentAsync(recipeId, userId, dto);
                return CreatedAtAction(nameof(GetComments), new { recipeId }, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании комментария");
                if (ex.Message.Contains("Слишком много комментариев"))
                    return StatusCode(429, new { error = ex.Message });
                if (ex.Message.Contains("не найден"))
                    return NotFound(new { error = ex.Message });
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{commentId}/reply")]
        public async Task<IActionResult> ReplyToComment(int commentId, [FromBody] CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var comment = await _commentService.ReplyToCommentAsync(commentId, userId, dto);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ответе на комментарий");
                if (ex.Message.Contains("Слишком много комментариев"))
                    return StatusCode(429, new { error = ex.Message });
                if (ex.Message.Contains("не найден"))
                    return NotFound(new { error = ex.Message });
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var comment = await _commentService.UpdateCommentAsync(id, userId, dto);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении комментария");
                if (ex.Message.Contains("не найден"))
                    return NotFound(new { error = ex.Message });
                if (ex.Message.Contains("Вы можете редактировать только свои"))
                    return Forbid();
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            try
            {
                await _commentService.DeleteCommentAsync(id, userId, isAdmin);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении комментария");
                if (ex.Message.Contains("не найден"))
                    return NotFound(new { error = ex.Message });
                if (ex.Message.Contains("Вы можете удалять только свои"))
                    return Forbid();
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{commentId}/complaints")]
        public async Task<IActionResult> CreateComplaint(int commentId, [FromBody] CreateComplaintDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var complaint = await _complaintService.CreateComplaintAsync(commentId, userId, dto);
                return Ok(complaint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании жалобы");
                if (ex.Message.Contains("уже отправляли"))
                    return Conflict(new { error = ex.Message });
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}