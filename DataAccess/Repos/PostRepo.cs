using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Utilities;

using Domain.Entities;
using Domain.Shared;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repos;

public class PostRepo : IPostRepo
{
    private readonly BlogContext _context;
    private readonly ILogger<PostRepo> _logger;
    private readonly IMapper _mapper;

    public PostRepo(BlogContext context, ILogger<PostRepo> logger, IMapper mapper)
    {
        _mapper = mapper;
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


    public async Task<Result<List<ArchivePostDto>>> GetArchivePosts()
    {
        var res = await  _context.Posts.ToListAsync();

        try
        {
            if (res == null)
                return Result<List<ArchivePostDto>>.Failure("No Posts found", OperationStatus.NotFound);

            return Result <List<ArchivePostDto>>.Success( _mapper.Map<List<ArchivePostDto>>(res));

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<List<ArchivePostDto>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<Dictionary<string, List<PostReadDto>>>> GetAllPostsGroupedByCategory()
    {
        try
        {

            var res = await _context.Posts
            .Include(p=>p.PostCategories)
            .ProjectTo<PostReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

            if (res == null)
                    return Result<Dictionary<string, List<PostReadDto>>>.Failure("No Posts found", OperationStatus.NotFound);

                var groupedPosts = new Dictionary<string, List<PostReadDto>>();
            foreach (var post in res)
            {
                foreach (var catName in post.Categories)
                {

                    if (!groupedPosts.ContainsKey(catName) )
                    {
                        groupedPosts[catName] = new List<PostReadDto>() { post};

                    }
                    else
                        groupedPosts[catName].Add(post);
                }
            }



            return Result<Dictionary<string, List<PostReadDto>>>.Success(groupedPosts);

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<Dictionary<string, List<PostReadDto>>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<PagedList<PostReadDto>>> GetAll(
     int page = 1,
     int pageSize = 10,
     string? searchTerm = null,
     string sortBy = "publish_date",
     bool isSortAscending = true,
     string? filterValue = null,
     string? filterType = null)
    {
        try
        {
            var query = _context.Posts
                .AsNoTracking();

            if (pageSize > 10 || pageSize < 1)
                pageSize = 10;

            // Apply filtering based on filter type
            if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(filterValue))
                ApplyFilter(ref query, filterValue, filterType);

            // Apply additional filtering based on search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    p.Content.Contains(searchTerm));
            }

            // Apply dynamic sorting
            ApplySorting(ref query, sortBy, isSortAscending);

            // Paging
            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<PostReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = new PagedList<PostReadDto>(
                data: posts,
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

    private void ApplyFilter(ref IQueryable<Post> query, string filterValue, string filterType)
    {
            query = filterType.ToLower() switch
            {
                "tag" => query
                    .Where(p => p.PostTags.Any(pt => pt.Tag.Name == filterValue)), 
                "category" => query
                    .Where(p => p.PostCategories.Any(pc => pc.Category.Name == filterValue)), 
                _ => query, 
            };
    }


    private void ApplySorting(ref IQueryable<Post> query, string sortBy, bool isSortAscending)
    {
        var propertyMap = new Dictionary<string, Expression<Func<Post, object>>>
        {
            ["title"] = p => p.Title,
            ["content"] = p => p.Content,
            ["publish_date"] = p => p.PublishDate,
        };

        if (propertyMap.TryGetValue(sortBy.ToLower(), out var orderBy))
        {
            query = isSortAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
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
                .SingleOrDefaultAsync(t => t.Name == tag.Name.Trim().Replace(' ', '-'));

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
                .SingleOrDefaultAsync(t => t.Name == category.Name.Trim().Replace(' ', '-'));

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