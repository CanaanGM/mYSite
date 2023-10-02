using System.Text.RegularExpressions;
using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Dtos;
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
                .SingleOrDefaultAsync(p => p.Slug == slug);
            return Result<PostReadDto>.Success(_mapper.Map<PostReadDto>(post));
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
                    p.Body.Contains(searchTerm));
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
                .ToListAsync();

            var result = new PagedList<PostReadDto>(
                data: _mapper.Map<List<PostReadDto>>(posts),
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
            // .Include()
            .SingleOrDefaultAsync(p => p.Id == postId);
        if (post2Update is not Post)
            return await CreatePost(postDto);


        post2Update.Slug = GenerateSlug(postDto.Title);
        _mapper.Map(postDto, post2Update);
        // fix relations here !
        await _context.SaveChangesAsync();

        return Result<PostReadDto>.Success(_mapper.Map<PostReadDto>(post2Update), OperationStatus.Updated);
    }

    public async Task<Result<PostReadDto>> CreatePost(PostUpsertDto postDto)
    {
        try
        {
            var newPost = _mapper.Map<Post>(postDto);
            newPost.Slug = GenerateSlug(newPost.Title);


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


    private string GenerateSlug(string title)
    {
        string sanitize = Regex.Replace(title.ToLower().Trim(), @"[^a-zA-Z0-9]+", "-");
        string slug = Regex.Replace(sanitize, @"-$", "");
        return slug;
    }

}