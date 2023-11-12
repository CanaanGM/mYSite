// Ignore Spelling: blog Repo Repos upsert

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Entities;
using DataAccess.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Azure.Core.HttpHeader;

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
                                      join user in _blogContext.Users on comment.AuthorId equals user.Id
                                      where comment.PostId == postId
                                      select new CommentReadDto
                                      {
                                          Id = comment.Id,
                                          Body = comment.Body,
                                          Active = comment.Active,
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
                                      }).ToListAsync<CommentReadDto>();

                return Result<IEnumerable<CommentReadDto>>.Success(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Result<IEnumerable<CommentReadDto>>.Failure(ex.Message, OperationStatus.Error);
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


        private async Task<Result<CommentReadDto>> CreateComment(CommentCreateDto newComment, User author)
        {
            try
            {
                var comment = new Comment
                {
                    Id = newComment.Id,
                    AuthorId =  author.Id,
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
