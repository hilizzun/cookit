using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CookIt.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IComplaintRepository _complaintRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly IMinioImageStorage _imageStorage;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ICommentRepository commentRepository,
            IComplaintRepository complaintRepository,
            IRecipeRepository recipeRepository,
            IUserStatisticsService userStatisticsService,
            IMinioImageStorage imageStorage,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _complaintRepository = complaintRepository;
            _userStatisticsService = userStatisticsService;
            _recipeRepository = recipeRepository;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsForRecipeAsync(int recipeId, int pageNumber, int pageSize)
        {
            var comments = await _commentRepository.GetCommentsByRecipeIdAsync(recipeId, pageNumber, pageSize);
            var dtos = new List<CommentDto>();
            foreach (var comment in comments)
            {
                dtos.Add(await MapToDto(comment));
            }
            return dtos;
        }

        public async Task<CommentDto> AddCommentAsync(int recipeId, string userId, CreateCommentDto dto)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId);
            if (recipe == null)
                throw new Exception("Рецепт не найден");

            var count = await _commentRepository.GetUserCommentCountWithinTimeSpanAsync(userId, TimeSpan.FromMinutes(1));
            if (count >= 3)
                throw new Exception("Слишком много комментариев. Пожалуйста, подождите минуту.");

            var comment = new Comment
            {
                Content = dto.Content,
                RecipeId = recipeId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = dto.ParentCommentId
            };

            var created = await _commentRepository.AddCommentAsync(comment);
            var fullComment = await _commentRepository.GetCommentByIdAsync(created.Id);
            await _userStatisticsService.IncrementCommentsLeftAsync(userId);
            return await MapToDto(fullComment);
        }

        public async Task<CommentDto> ReplyToCommentAsync(int parentId, string userId, CreateCommentDto dto)
        {
            var parent = await _commentRepository.GetCommentByIdAsync(parentId);
            if (parent == null)
                throw new Exception("Родительский комментарий не найден");

            var count = await _commentRepository.GetUserCommentCountWithinTimeSpanAsync(userId, TimeSpan.FromMinutes(1));
            if (count >= 3)
                throw new Exception("Слишком много комментариев. Пожалуйста, подождите минуту.");

            var comment = new Comment
            {
                Content = dto.Content,
                RecipeId = parent.RecipeId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = parentId
            };

            var created = await _commentRepository.AddCommentAsync(comment);
            var fullComment = await _commentRepository.GetCommentByIdAsync(created.Id);
            return await MapToDto(fullComment);
        }

        public async Task<CommentDto> UpdateCommentAsync(int commentId, string userId, UpdateCommentDto dto)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Комментарий не найден");

            if (comment.UserId != userId)
                throw new Exception("Вы можете редактировать только свои комментарии");

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepository.UpdateCommentAsync(comment);
            var updated = await _commentRepository.GetCommentByIdAsync(commentId);
            return await MapToDto(updated);
        }

        public async Task DeleteCommentAsync(int commentId, string userId, bool isAdmin)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null) throw new Exception("Комментарий не найден");
            if (comment.UserId != userId && !isAdmin) throw new Exception("Вы можете удалять только свои комментарии");
            if (comment.IsDeleted) return;

            await _commentRepository.SoftDeleteCommentAsync(commentId);
            await _userStatisticsService.DecrementCommentsLeftAsync(comment.UserId);

            var complaints = await _complaintRepository.GetByCommentIdAsync(commentId);
            foreach (var complaint in complaints)
            {
                complaint.Status = ComplaintStatus.Resolved;
                complaint.ResolvedByUserId = userId;
                complaint.ResolvedAt = DateTime.UtcNow;
                complaint.ResolutionNote = "Комментарий удалён";
                await _complaintRepository.UpdateAsync(complaint);
            }
        }

        private async Task<CommentDto> MapToDto(Comment comment)
        {
            string? avatarUrl = null;
            if (comment.User?.AvatarKey != null)
            {
                try
                {
                    avatarUrl = await _imageStorage.GetPreviewUrlAsync(comment.User.AvatarKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении URL аватара");
                }
            }

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId,
                UserName = comment.User?.UserName ?? "Неизвестный",
                UserAvatarUrl = avatarUrl,
                RecipeId = comment.RecipeId,
                ParentCommentId = comment.ParentCommentId,
                IsDeleted = comment.IsDeleted
            };
        }
    }
}