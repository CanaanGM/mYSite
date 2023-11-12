using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Entities;
using DataAccess.Shared;
using DataAccess.Utilities;

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

        public TagRepo(BlogContext context, ILogger<TagRepo> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public IMapper _mapper { get; }

        public async Task<Result<List<TagReadDto>>> GetAllTags()
        {
            try
            {
                var tags = await _context.Tags
                    .AsNoTracking()

                    //.Include(x=>x.PostTags)
                    //.ThenInclude(x=>x.Post)


                    .ProjectTo<TagReadDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                // 404
                return Result<List<TagReadDto>>.Success(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR: {ex.Message}", ex);
                return Result<List<TagReadDto>>.Failure($"Failed to get or create the tag: {ex.Message}");
            }
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

        public async Task<Result<TagReadDto>> GetTagByName(string name)
        {
            try
            {
                var tag = await _context.Tags
                    .AsNoTracking()
                    .ProjectTo<TagReadDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(x => x.Name == name);
                    ;

                // 404 implement powlease
                return Result<TagReadDto>.Success(tag);

            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR: {ex.Message}", ex);
                return Result<TagReadDto>.Failure($"Failed to get or create the tag: {ex.Message}");
            }
        }
    }


}
