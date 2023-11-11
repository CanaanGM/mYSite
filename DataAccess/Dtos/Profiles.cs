using AutoMapper;

using DataAccess.Entities;

using Domain.Entities;

namespace DataAccess.Dtos;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<PostReadDto, Post>().ReverseMap()

         .ForMember(dest => dest.Tags,
         opt => opt
         .MapFrom(src => src.PostTags.Select(pt => pt.Tag.Name))
         )
         .ForMember(dest => dest.Categories,
            opt => opt.MapFrom(src =>
            src.PostCategories.Select(pt => pt.Category.Name)))
        ;
        CreateMap<Post, PostUpsertDto>().ReverseMap();
        CreateMap<Post, ArchivePostDto>().ReverseMap();
        CreateMap<Post, PostReadWithTag>().ReverseMap();

        CreateMap<TagReadDto, Tag>()
            .ReverseMap()
            .ForMember(x => x.Posts, opt =>
                opt.MapFrom(s => s.PostTags.Select(x => x.Post)))
            ;
            
        CreateMap<Tag, TagUpsertDto>().ReverseMap();


        CreateMap<Category, CategoryReadDto>().ReverseMap();
        CreateMap<Category, CategoryUpsertDto>().ReverseMap();


        CreateMap<Comment, CommentCreateDto>().ReverseMap();
        CreateMap<Comment, CommentUpdateDto>().ReverseMap();
        CreateMap<Comment, CommentReadDto>().ReverseMap();

        CreateMap<User, UserReadDto>().ReverseMap();
        CreateMap<User, CommentReadDto>()
            .ForMember(x => x.UserName, opt => opt.MapFrom(a => a.UserName))
            .ForMember(x => x.Email, opt => opt.MapFrom(a => a.Email))
            .ForMember(x => x.ProfilePicture, opt => opt.MapFrom(a => a.ProfilePicture));


    }
}