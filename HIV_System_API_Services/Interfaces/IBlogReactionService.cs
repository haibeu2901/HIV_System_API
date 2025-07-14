using HIV_System_API_DTOs.BlogReactionDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IBlogReactionService
    {
        Task<CommentResponseDTO> CreateCommentAsync(CommentRequestDTO dto);
        Task<CommentResponseDTO?> GetCommentByIdAsync(int id);
        Task<List<CommentResponseDTO>> GetCommentsByBlogIdAsync(int blogId);
        Task<CommentResponseDTO> UpdateCommentAsync(int id, UpdateCommentRequestDTO dto);
        Task<bool> DeleteCommentAsync(int id);
    }
}
