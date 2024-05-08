using Data.Models;
using Microsoft.EntityFrameworkCore;
using Services.Repositories.Interfaces;

namespace Services.Repositories.Implements
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _db;

        public CommentRepository(ApplicationDbContext db) {
            _db = db;
        }
        

        public async Task<List<Comment>> GetAllAsync()
        {
            return await _db.Comments.Include(a => a.User).ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _db.Comments.Include("AppUser").FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Comment?> CreateAsync(Comment commentModel)
        {
            await _db.Comments.AddAsync(commentModel);
            await _db.SaveChangesAsync();
            return commentModel;
        }

        public async Task<Comment?> DeleteAsync(int id)
        {
            var commentModel = await GetByIdAsync(id);
            if (commentModel is null)
                return null;

            _db.Comments.Remove(commentModel);
            await _db.SaveChangesAsync();
            return commentModel;
        }

        public async Task<Comment?> UpdateAsync(int id, Comment commentDto)
        {
            var commentModel = await GetByIdAsync(id);
            if (commentModel is null)
                return null;

            commentModel.Tiltle = commentDto.Tiltle;
            commentModel.Content = commentDto.Content;

            await _db.SaveChangesAsync();
            return commentModel;
        }
       
    }
}
