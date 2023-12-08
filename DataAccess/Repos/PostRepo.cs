// Ignore Spelling: Repo

using System.Collections.Generic;
using System.Linq.Expressions;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Entities;
using DataAccess.Shared;
using DataAccess.Utilities;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task<Result<PostReadDetailsDto>> GetBySlug(string slug)
    {
        try
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Include(w => w.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .Include(x => x.PostTags)
                    .ThenInclude(x => x.Tag)
                .Include(x => x.PostCategories)
                    .ThenInclude(x => x.Category)
                .ProjectTo<PostReadDetailsDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(x => x.Slug == slug);

            if (post is null) return Result<PostReadDetailsDto>.Failure("Post was not found", OperationStatus.NotFound);

            var post2 = await _context.Posts
                .Include(x => x.UserReactions)
                .Where(x => x.Slug == slug)
                .SingleOrDefaultAsync();

            if (post != null)
            {
                post.Likes = post2.UserReactions.Count(ur => ur.ReactionType == ReactionType.Like);
                post.Dislikes = post2.UserReactions.Count(ur => ur.ReactionType == ReactionType.Dislike);
            }

            return Result<PostReadDetailsDto>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PostReadDetailsDto>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<List<ArchivePostDto>>> GetArchivePosts()
    {
        var res = await _context.Posts.ToListAsync();

        try
        {
            if (res == null)
                return Result<List<ArchivePostDto>>.Failure("No Posts found", OperationStatus.NotFound);

            return Result<List<ArchivePostDto>>.Success(_mapper.Map<List<ArchivePostDto>>(res));
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<List<ArchivePostDto>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<Dictionary<string, List<PostGeneralInfoDto>>>> GetAllPostsGroupedByCategory()
    {
        try
        {
            var res = await _context.Posts
            .Include(p => p.PostCategories)
            .ProjectTo<PostGeneralInfoDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

            if (res == null)
                return Result<Dictionary<string, List<PostGeneralInfoDto>>>.Failure("No Posts found", OperationStatus.NotFound);

            var groupedPosts = new Dictionary<string, List<PostGeneralInfoDto>>();
            foreach (var post in res)
            {
                foreach (var catName in post.Categories)
                {
                    if (!groupedPosts.ContainsKey(catName))
                    {
                        groupedPosts[catName] = new List<PostGeneralInfoDto>() { post };
                    }
                    else
                        groupedPosts[catName].Add(post);
                }
            }

            return Result<Dictionary<string, List<PostGeneralInfoDto>>>.Success(groupedPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<Dictionary<string, List<PostGeneralInfoDto>>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<PagedList<PostGeneralInfoDto>>> GetAll(
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

            if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(filterValue))
                ApplyFilter(ref query, filterValue, filterType);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm)
                    || p.Content.ToString()!.Contains(searchTerm)

                    );
            }

            ApplySorting(ref query, sortBy, isSortAscending);

            var posts = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(post => new PostGeneralInfoDto
                {
                    Id = post.Id,
                    Title = post.Title,
                    Slug = post.Slug,
                    Content = post.Content,
                    IsSoftDeleted = post.IsSoftDeleted,
                    Likes = post.UserReactions.Count(ur => ur.ReactionType == ReactionType.Like),
                    Dislikes = post.UserReactions.Count(ur => ur.ReactionType == ReactionType.Dislike),
                    Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                    Categories = post.PostCategories.Select(pc => pc.Category.Name).ToList(),
                    IsPublished = post.IsPublished,
                    PublishDate = post.PublishDate,
                    CreatedAt = post.CreatedAt,
                    LastModifiedAt = post.LastModifiedAt
                })
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = new PagedList<PostGeneralInfoDto>(
                data: posts,
                totalCount: totalCount,
                totalPages: totalPages,
                currentPage: page,
                pageSize: pageSize);

            return Result<PagedList<PostGeneralInfoDto>>.Success(result, OperationStatus.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PagedList<PostGeneralInfoDto>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<PagedList<PostReadWithEntity>>> GetUsersFavoritePosts(
        string userId,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            if (pageSize > 10 || pageSize < 1)
                pageSize = 10;

            var query = _context.UsersFavoritePosts
                .AsNoTracking()
                .Include(x => x.Post)
                .Where(x => x.UserId == userId);

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Post)
                .OrderBy(s => s.CreatedAt)
                .ProjectTo<PostReadWithEntity>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            PagedList<PostReadWithEntity>? result = new PagedList<PostReadWithEntity>(
                data: posts,
                totalCount: totalCount,
                totalPages: totalPages,
                currentPage: page,
                pageSize: pageSize);

            return Result<PagedList<PostReadWithEntity>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PagedList<PostReadWithEntity>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<bool>> UpSertPostReaction(string userId, Guid postId, ReactionType reactionType)
    {
        try
        {
            var existingReaction = await _context.PostsUsersReaction
                .FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);

            Result<bool> res;

            if (existingReaction is not null && existingReaction?.ReactionType != reactionType)
            {
                existingReaction.ReactionType = reactionType;
                _context.PostsUsersReaction.Add(existingReaction);

                res = Result<bool>.Success(true, OperationStatus.Updated);
            }
            else if (existingReaction?.ReactionType == reactionType)
            {
                _context.PostsUsersReaction.Remove(existingReaction);
                res = Result<bool>.Success(true, OperationStatus.Deleted);
            }
            else
            {
                var newReaction = new PostUserReaction
                {
                    UserId = userId,
                    PostId = postId,
                    ReactionType = reactionType
                };
                _context.PostsUsersReaction.Add(newReaction);
                res = Result<bool>.Success(true, OperationStatus.Created);
            }

            var result = await _context.SaveChangesAsync() > 0;

            return result
                ? res
                : Result<bool>.Failure("Failed to create Reaction", OperationStatus.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result<bool>.Failure(ex.Message, OperationStatus.Error);
        }
    }

    public async Task<Result<PostReadDetailsDto>> UpsertPost(string authorId, Guid? postId, PostUpsertDto postDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == authorId);

        if (user is null)
            return Result<PostReadDetailsDto>.Failure("Couldn't find user", OperationStatus.NotFound);

        var post2Update = await _context.Posts
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostCategories)
                .ThenInclude(pt => pt.Category)

            .SingleOrDefaultAsync(p => p.Id == postId);

        if (post2Update is not Post)
        {
            postDto.AuthorId = user.Id;
            return await CreatePost(postDto);
        }

        post2Update.AuthorId = user.Id;
        return await UpdatePost(postDto, post2Update);
    }

    private async Task<Result<PostReadDetailsDto>> UpdatePost(PostUpsertDto postDto, Post post2Update)
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

            return Result<PostReadDetailsDto>.Success(_mapper.Map<PostReadDetailsDto>(post2Update), OperationStatus.Updated);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PostReadDetailsDto>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<PostReadDetailsDto>> CreatePost(PostUpsertDto postDto)
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

            return Result<PostReadDetailsDto>.Success(
                _mapper.Map<PostReadDetailsDto>(newPost), OperationStatus.Created);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<PostReadDetailsDto>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<bool>> SoftDelete(Guid postId)
    {
        try
        {
            var post = await _context.Posts.FirstAsync(p => p.Id == postId);

            post.IsSoftDeleted = true;

            var res = await _context.SaveChangesAsync() > 0;

            return res
                ? Result<bool>.Success(true, OperationStatus.Deleted)
                : Result<bool>.Failure("problem deleting", OperationStatus.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<bool>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
        }
    }

    public async Task<Result<bool>> HardDelete(Guid postId)
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
}