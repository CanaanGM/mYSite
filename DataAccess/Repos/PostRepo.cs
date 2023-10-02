using System.Text.RegularExpressions;
using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Dtos;
using Domain.Entities;
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

    public async Task<Result<List<PostReadDto>>> GetAll()
    {
        try
        {
            var post = await _context.Posts
                .AsNoTracking()
                .ToListAsync();

            return Result<List<PostReadDto>>.Success(_mapper.Map<List<PostReadDto>>(post));
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: {ex.Message}", ex);
            return Result<List<PostReadDto>>.Failure($"ERROR: {ex.Message}", OperationStatus.Error);
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