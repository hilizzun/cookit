using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CookIt.Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ComplaintService> _logger;

        public ComplaintService(
            IComplaintRepository complaintRepository,
            ICommentRepository commentRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<ComplaintService> logger)
        {
            _complaintRepository = complaintRepository;
            _commentRepository = commentRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ComplaintDto> CreateComplaintAsync(int commentId, string userId, CreateComplaintDto dto)
        {
            var existing = await _complaintRepository.HasUserComplainedAsync(commentId, userId);
            if (existing)
                throw new Exception("Вы уже отправляли жалобу на этот комментарий");

            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Комментарий не найден");

            var complaint = new Complaint
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Reason = dto.Reason,
                Status = ComplaintStatus.Pending
            };

            var created = await _complaintRepository.AddAsync(complaint);
            return await MapToDto(created);
        }

        public async Task<IEnumerable<ComplaintDto>> GetPendingComplaintsAsync()
        {
            var complaints = await _complaintRepository.GetPendingAsync();
            var dtos = new List<ComplaintDto>();
            foreach (var c in complaints)
            {
                dtos.Add(await MapToDto(c));
            }
            return dtos;
        }

        public async Task ResolveComplaintAsync(int complaintId, string adminUserId, ResolveComplaintDto dto)
        {
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
                throw new Exception("Жалоба не найдена");

            if (dto.DeleteComment)
            {
                await _commentRepository.SoftDeleteCommentAsync(complaint.CommentId);
                complaint.Status = ComplaintStatus.Resolved;
            }
            else
            {
                complaint.Status = ComplaintStatus.Rejected;
            }

            complaint.ResolvedByUserId = adminUserId;
            complaint.ResolvedAt = DateTime.UtcNow;
            complaint.ResolutionNote = dto.ResolutionNote;

            await _complaintRepository.UpdateAsync(complaint);
        }

        private async Task<ComplaintDto> MapToDto(Complaint complaint)
        {
            var user = await _userManager.FindByIdAsync(complaint.UserId);
            var resolvedBy = complaint.ResolvedByUserId != null
                ? await _userManager.FindByIdAsync(complaint.ResolvedByUserId)
                : null;

            var comment = complaint.Comment;
            if (comment == null)
                comment = await _commentRepository.GetCommentByIdAsync(complaint.CommentId);

            return new ComplaintDto
            {
                Id = complaint.Id,
                CommentId = complaint.CommentId,
                CommentContent = comment?.Content ?? "Комментарий удалён",
                UserId = complaint.UserId,
                UserName = user?.UserName ?? "Неизвестный",
                CreatedAt = complaint.CreatedAt,
                Reason = complaint.Reason,
                Status = complaint.Status.ToString(),
                ResolvedByUserName = resolvedBy?.UserName,
                ResolvedAt = complaint.ResolvedAt,
                ResolutionNote = complaint.ResolutionNote,
                RecipeId = comment?.RecipeId ?? 0
            };
        }
    }
}