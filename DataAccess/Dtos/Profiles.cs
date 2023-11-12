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
        CreateMap<Post, PostReadWithTag>();

        CreateMap<Tag, TagReadDto>();

        CreateMap<Category, CategoryReadDto>();

        CreateMap<User, UserReadDto>();

        CreateMap<Comment, CommentReadDto>();





    }
}