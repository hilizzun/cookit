using CookIt.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CookIt.Core.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsForRecipeAsync(int recipeId, int pageNumber, int pageSize);
        Task<CommentDto> AddCommentAsync(int recipeId, string userId, CreateCommentDto dto);
        Task<CommentDto> ReplyToCommentAsync(int parentId, string userId, CreateCommentDto dto);
        Task<CommentDto> UpdateCommentAsync(int commentId, string userId, UpdateCommentDto dto);
        Task DeleteCommentAsync(int commentId, string userId, bool isAdmin);
    }
}