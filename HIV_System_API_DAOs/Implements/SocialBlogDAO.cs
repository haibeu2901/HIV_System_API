using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace HIV_System_API_DAOs.Implements
{
    public class SocialBlogDAO : ISocialBlogDAO
    {
        private readonly HivSystemApiContext _context;
        private static SocialBlogDAO _instance;
        public static SocialBlogDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SocialBlogDAO();
                }
                return _instance;
            }
        }

        private SocialBlogDAO() 
        { 
            _context = new HivSystemApiContext();
        }

        public async Task<List<SocialBlog>> GetAllAsync()
        {
            return await _context.SocialBlogs.ToListAsync();
        }

        public async Task<SocialBlog?> GetByIdAsync(int id)
        {
            return await _context.SocialBlogs.FindAsync(id);
        }

        public async Task<List<SocialBlog?>> GetByAuthorIdAsync(int id)
        {
            return await _context.SocialBlogs
                .Where(b => b.AccId == id)
                .ToListAsync();
        }

        public async Task<SocialBlog> CreateAsync(SocialBlog blog)
        {
            _context.SocialBlogs.Add(blog);
            await _context.SaveChangesAsync();
            return blog;
        }

        public async Task<SocialBlog> UpdateAsync(int id, SocialBlog blog)
        {
            var existing = await _context.SocialBlogs.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            existing.Title = blog.Title;
            existing.Content = blog.Content;
            existing.IsAnonymous = blog.IsAnonymous;
            existing.Notes = blog.Notes;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<SocialBlog> UpdatePersonalAsync(int blogId, int authorId, SocialBlog blog)
        {
            var existing = await _context.SocialBlogs.FindAsync(blogId);
            if (existing == null || existing.AccId != authorId)
                throw new KeyNotFoundException($"Blog with ID {blogId} not found or does not belong to author with ID {authorId}.");
            existing.Title = blog.Title;
            existing.Content = blog.Content;
            existing.IsAnonymous = blog.IsAnonymous;
            existing.Notes = blog.Notes;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var blog = await _context.SocialBlogs.FindAsync(id);
            if (blog == null)
                return false;

            _context.SocialBlogs.Remove(blog);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SocialBlog> UpdateBlogStatusAsync(int id, byte blogStatus, int staffId, string? notes)
        {
            var blog = await _context.SocialBlogs.FindAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            blog.BlogStatus = blogStatus;
            blog.StfId = staffId;
            if (notes != null)
                blog.Notes = notes;

            await _context.SaveChangesAsync();
            return blog;
        }

        public async Task<SocialBlog> CreateDraftAsync(SocialBlog draft)
        {
            draft.StfId = null; 
            draft.BlogStatus = 1;
            draft.PublishedAt = null;
            await _context.SocialBlogs.AddAsync(draft);
            await _context.SaveChangesAsync();
            return draft;
        }

        public async Task<List<SocialBlog>> GetDraftsByAuthorIdAsync(int authorId)
        {
            return await _context.SocialBlogs
                .Where(b => b.AccId == authorId && b.BlogStatus == 0)
                .ToListAsync();
        }
    }
}
