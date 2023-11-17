// Ignore Spelling: blog Repo Repos upsert

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Entities;
using DataAccess.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repos
{
    public class CommentRepo : ICommentRepo
    {
        private readonly BlogContext _blogContext;
        private readonly ILogger<PostRepo> _logger;
        private readonly IMapper _mapper;

        public CommentRepo(BlogContext blogContext, ILogger<PostRepo> logger, IMapper mapper)
        {
            _blogContext = blogContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<CommentReadDto>>> GetCommentsForPost(Guid postId)
        {
            try
            {
                var comments = await (from comment in _blogContext.Comments
                                      join userReaction in _blogContext.CommentsUsersReactions on comment.Id equals userReaction.CommentId
                                      join user in _blogContext.Users on userReaction.UserId equals user.Id
                                      where comment.PostId == postId && comment.AuthorId == user.Id
                                      select new CommentReadDto
                                      {
                                          Id = comment.Id,
                                          Body = comment.Body,
                                          Active = comment.Active,
                                          Likes = _blogContext.CommentsUsersReactions.Count(ur => ur.CommentId == comment.Id && ur.ReactionType == ReactionType.Like),
                                          Dislikes = _blogContext.CommentsUsersReactions.Count(ur => ur.CommentId == comment.Id && ur.ReactionType == ReactionType.Dislike),
                                          Author = new UserReadDto
                                          {
                                              Id = user.Id,
                                              Email = user.Email!,
                                              UserName = user.UserName!,
                                              ProfilePicture = user.ProfilePicture,
                                          },
                                          ParentId = comment.parentId,
                                          PostId = comment.PostId,
                                          Updated = comment.Updated,
                                          Created = comment.Created
                                      })
                                      .OrderBy(x => x.Created)
                                      .ToListAsync<CommentReadDto>();

                return Result<IEnumerable<CommentReadDto>>.Success(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Result<IEnumerable<CommentReadDto>>.Failure(ex.Message, OperationStatus.Error);
            }
        }

        public async Task<Result<PagedList<CommentReadForAuthorDto>>> GetAllCommentsForUser(
            string userId,
            int page = 1,
            int pageSize = 10
            )
        {
            try
            {
                var query = _blogContext.Comments
                    .AsNoTracking()
                    .Include(x => x.Post);
                    // do i need the replies ? ? ?

                var comments = await query
                    .Where(x => x.AuthorId == userId)
                    .Skip((page -1 ) * pageSize)
                    .Take(pageSize)
                    .OrderBy(x => x.Created)
                    .ProjectTo<CommentReadForAuthorDto>(_mapper.ConfigurationProvider)
                .ToListAsync();


                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var result = new PagedList<CommentReadForAuthorDto>(
                    data: comments,
                    totalCount: totalCount,
                    totalPages: totalPages,
                    currentPage: page,
                    pageSize: pageSize
                    );

                return Result<PagedList<CommentReadForAuthorDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Result<PagedList<CommentReadForAuthorDto>>.Failure(ex.Message, OperationStatus.Error);
            }
        }

        public async Task<Result<CommentReadDto>> UpsertComment(CommentCreateDto upsertComment, string authorId)
        {
            try
            {
                var author = await _blogContext.Users.FirstAsync(a => a.Id == authorId);
                if (author is null)
                    return Result<CommentReadDto>.Failure("Failed to find user . . .", OperationStatus.NotFound);

                var comment = await _blogContext.Comments.FirstOrDefaultAsync(x => x.Id == upsertComment.Id);

                if (comment is null || comment.AuthorId != author.Id)
                    return await CreateComment(upsertComment, author);

                comment.Body = upsertComment.Body ?? comment.Body;

                var res = await _blogContext.SaveChangesAsync() > 0;
                return res
                    ? Result<CommentReadDto>.Success(_mapper.Map<CommentReadDto>(comment), OperationStatus.Updated)
                    : Result<CommentReadDto>.Failure("Couldn't update comment", OperationStatus.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Result<CommentReadDto>.Failure(ex.Message, OperationStatus.Error);
            }
        }

        public async Task<Result<bool>> SoftDeleteComment(Guid commentId, string authorId)
        {
            var comment = await _blogContext.Comments.FirstAsync(x => x.Id == commentId);

            if (comment == null || comment.AuthorId != authorId)
                return Result<bool>.Failure("Comment not found", OperationStatus.NotFound);

            comment.Active = false;

            var res = await _blogContext.SaveChangesAsync() > 0;
            return res
                ? Result<bool>.Success(true, OperationStatus.Deleted)
                : Result<bool>.Failure("Couldn't update comment", OperationStatus.Error);
        }

        public async Task<Result<bool>> UpsertCommentReaction(string userId, Guid commentId, ReactionType reactionType)
        {
            try
            {
                var existingReaction = await _blogContext.CommentsUsersReactions
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.CommentId == commentId);

                Result<bool> res;

                if (existingReaction is not null && existingReaction?.ReactionType != reactionType)
                    res = UpdateReaction(existingReaction, reactionType);
                else if (existingReaction?.ReactionType == reactionType)
                    res = RemoveReaction(existingReaction);
                else
                    res = CreateReaction(userId, commentId, reactionType);

                var result = await _blogContext.SaveChangesAsync() > 0;

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

        private Result<bool> CreateReaction(string userId, Guid commentId, ReactionType reactionType)
        {
            var newReaction = new CommentUserReaction
            {
                UserId = userId,
                CommentId = commentId,
                ReactionType = reactionType
            };
            _blogContext.CommentsUsersReactions.Add(newReaction);
            return Result<bool>.Success(true, OperationStatus.Created);
        }

        private Result<bool> RemoveReaction(CommentUserReaction existingReaction)
        {
            _blogContext.CommentsUsersReactions.Remove(existingReaction);
            return Result<bool>.Success(true, OperationStatus.Deleted);
        }

        private Result<bool> UpdateReaction(CommentUserReaction existingReaction, ReactionType reactionType)
        {
            existingReaction.ReactionType = reactionType;
            _blogContext.CommentsUsersReactions.Add(existingReaction);
            return Result<bool>.Success(true, OperationStatus.Updated);
        }

        private async Task<Result<CommentReadDto>> CreateComment(CommentCreateDto newComment, User author)
        {
            try
            {
                var comment = new Comment
                {
                    Id = newComment.Id,
                    AuthorId = author.Id,
                    Active = true,
                    parentId = newComment.ParentId,
                    PostId = newComment.PostId,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    Body = newComment.Body,
                };

                await _blogContext.Comments.AddAsync(comment);
                var res = await _blogContext.SaveChangesAsync() > 0;

                return res
                    ? Result<CommentReadDto>.Success(
                        new CommentReadDto
                        {
                            Id = newComment.Id,
                            Author = new UserReadDto
                            {
                                Id = author.Id,
                                Email = author.Email!,
                                UserName = author.UserName!,
                                ProfilePicture = author.ProfilePicture,
                            },
                            Active = true,
                            ParentId = newComment.ParentId,
                            PostId = newComment.PostId,
                            Created = DateTime.UtcNow,
                            Updated = DateTime.UtcNow,
                            Body = newComment.Body,
                        }
                        , OperationStatus.Created)
                    : Result<CommentReadDto>.Failure("Couldn't create comment", OperationStatus.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Result<CommentReadDto>.Failure(ex.Message, OperationStatus.Error);
            }
        }
    }
}