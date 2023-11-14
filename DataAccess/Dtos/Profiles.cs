using AutoMapper;

using DataAccess.Entities;

namespace DataAccess.Dtos;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Post, PostReadDetailsDto>()
             .ForMember(x => x.Tags, opt => opt.MapFrom(x => x.PostTags.Select(x => x.Tag)))
            .ForMember(x => x.Categories, opt => opt.MapFrom(x => x.PostCategories.Select(x => x.Category)));

        CreateMap<Post, PostGeneralInfoDto>()
            .ForMember(x => x.Tags, opt => opt.MapFrom(x => x.PostTags.Select(s => s.Tag.Name)))
            .ForMember(x => x.Categories, opt => opt.MapFrom(x => x.PostCategories.Select(s => s.Category.Name)));

        CreateMap<Post, ArchivePostDto>();
        CreateMap<Post, PostReadWithEntity>();
        CreateMap<Post, PostUpsertDto>().ReverseMap();


        CreateMap<Tag, TagReadDto>()
            .ForMember(x => x.Posts, opt => opt.MapFrom(x => x.PostTags.Select(c => c.Post)))
            ;

        CreateMap<Category, CategoryReadDto>();

        CreateMap<User, UserReadDto>();

        CreateMap<Comment, CommentReadDto>();
        CreateMap<Comment, CommentReadForAuthorDto>()
            .ForMember(x => x.PostTitle, opt => opt.MapFrom(e => e.Post.Title))
            .ForMember(x => x.PostSlug, opt => opt.MapFrom(e => e.Post.Slug));




    }
}