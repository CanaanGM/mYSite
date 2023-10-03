using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Utilities;

using Domain.Entities;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repos;

public class PostRepo : IPostRepo
{
    private readonly BlogContext _context;
    private readonly ILogger<PostRepo> _logger;
    private readonly IMapper _mapper;
    private readonly ITagRepo _tagRepo;

    public PostRepo(BlogContext context, ILogger<PostRepo> logger, IMapper mapper, ITagRepo tagRepo)
    {
        _mapper = mapper;
        _tagRepo = tagRepo;
        _logger = logger;
        _context = context;
    }

    public async Task<Result<PostReadDto>> GetBySlug(string slug)
    {
        try
        {
            var post = await _context.Posts
                .AsNoTracking()
                .ProjectTo<PostReadDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Slug == slug);

            return Result<PostReadDto>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PostReadDto>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<PagedList<PostReadDto>>> GetAll(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string sortBy = "Id",
        bool isSortAscending = true)
    {
        
        try
        {
            var query = _context.Posts
                .AsNoTracking();

            // Filtering
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    p.Content.Contains(searchTerm));
            }

            // Sorting
            string sortOrder = isSortAscending ? "ASC" : "DESC";
            // will be an object later
            string orderBy = sortBy.ToLower() switch
            {
                "title" => "Title",
                "content" => "Content",
                _ => "Id",
            };


            // Paging
            var totalCount = await query.CountAsync();
            
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var posts = await query
                
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<PostReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PagedList<PostReadDto>(
                data:posts,
                totalCount: totalCount,
                totalPages: totalPages,
                currentPage: page,
                pageSize: pageSize);

            return Result<PagedList<PostReadDto>>.Success(result, OperationStatus.Success);

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PagedList<PostReadDto>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<PostReadDto>> UpsertPost(Guid postId, PostUpsertDto postDto)
    {
        var post2Update = await _context.Posts
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostCategories)
                .ThenInclude(pt => pt.Category)

            .SingleOrDefaultAsync(p => p.Id == postId);

        if (post2Update is not Post)
            return await CreatePost(postDto);

        return await UpdatePost(postDto, post2Update);

    }

    private async Task<Result<PostReadDto>> UpdatePost(PostUpsertDto postDto, Post post2Update)
    {
        try
        {
            post2Update.Slug = Sanitization.GenerateSlug(postDto.Title);
            _mapper.Map(postDto, post2Update);

            post2Update.LastModifiedAt = DateTime.UtcNow;
            post2Update.PostTags = new HashSet<PostTag>();
            post2Update.PostCategories = new HashSet<PostCategory>();

            if (postDto.IsPublished)
                post2Update.PublishDate = DateTime.UtcNow;
            else
                post2Update.PublishDate = new DateTime();


            await GeneratePostTags(postDto.Tags, post2Update);

            await GeneratePostCategories(postDto.Categories, post2Update);

            await _context.SaveChangesAsync();

            return Result<PostReadDto>.Success(_mapper.Map<PostReadDto>(post2Update), OperationStatus.Updated);

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PostReadDto>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<PostReadDto>> CreatePost(PostUpsertDto postDto)
    {
        try
        {
            var newPost = _mapper.Map<Post>(postDto);

            newPost.Slug = Sanitization.GenerateSlug(newPost.Title);
            newPost.PostTags = new HashSet<PostTag>();
            newPost.PostCategories = new HashSet<PostCategory>();
            newPost.LastModifiedAt = DateTime.UtcNow;
            if (postDto.IsPublished)
                newPost.PublishDate = DateTime.UtcNow;
            else
                newPost.PublishDate = new DateTime();

            await GeneratePostTags(postDto.Tags, newPost);
            await GeneratePostCategories(postDto.Categories, newPost);

            await _context.AddAsync(newPost);
            await _context.SaveChangesAsync();

            return Result<PostReadDto>.Success(
                _mapper.Map<PostReadDto>(newPost), OperationStatus.Created);

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PostReadDto>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }
   
   
    public async Task<Result<bool>> Delete(Guid postId)
    {
        try
        {
            var post = new Post { Id = postId };
            _context.Remove(post);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true, OperationStatus.Deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<bool>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    private async Task GeneratePostTags(ICollection<TagUpsertDto> tags, Post post2Update)
    {
        foreach (var tag in tags)
        {
            var existingTag = await _context.Tags
                .SingleOrDefaultAsync(t => t.Name == tag.Name);

            if (existingTag is null)
            {
                existingTag = new Tag
                {
                    Name = Sanitization.GenerateName(tag.Name),
                    LastModifiedAt = DateTime.UtcNow,
                };
                _context.Tags
                        .Add(existingTag);
            }

            post2Update.PostTags.Add(
                    new PostTag
                    {
                        Tag = existingTag,
                        Post = post2Update
                    });
        }
    }

    private async Task GeneratePostCategories(ICollection<CategoryUpsertDto> categories, Post post2Update)
    {
        foreach (var category in categories)
        {
            var existingCat = await _context.Categories
                .SingleOrDefaultAsync(t => t.Name == category.Name);

            if (existingCat is null)
            {
                existingCat = new Category
                {
                    Name = Sanitization.GenerateName(category.Name),
                    LastModifiedAt = DateTime.UtcNow
                };
                _context.Categories
                        .Add(existingCat);
            }

            post2Update.PostCategories.Add(
                    new PostCategory
                    {
                        Category = existingCat,
                        Post = post2Update
                    });
        }
    }



}