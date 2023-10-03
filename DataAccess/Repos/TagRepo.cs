using DataAccess.Contexts;
using DataAccess.Utilities;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAccess.Repos
{
    public class TagRepo : ITagRepo
    {
        private readonly BlogContext _context;
        private readonly ILogger<TagRepo> _logger;

        public TagRepo(BlogContext context, ILogger<TagRepo> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Result<Tag>> GetOrCreateTagAsync(string tagName)
        {
            try
            {
                var existingTag = await _context.Tags
                    .AsNoTracking()
                    .FirstOrDefaultAsync(tag => tag.Name == tagName);

                if (existingTag != null)
                {
                    return Result<Tag>.Success(existingTag);
                }

                var newTag = new Tag
                {
                    Name = Sanitization.GenerateName(tagName)
                };

                _context.Tags.Add(newTag);
                await _context.SaveChangesAsync();

                return Result<Tag>.Success(newTag, OperationStatus.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR: {ex.Message}", ex);
                return Result<Tag>.Failure($"Failed to get or create the tag: {ex.Message}");
            }
        }


    }


}
