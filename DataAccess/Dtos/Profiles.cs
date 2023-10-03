using AutoMapper;
using Domain.Entities;

namespace DataAccess.Dtos;
public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<PostReadDto, Post>().ReverseMap()

         .ForMember(dest => dest.Tags,
         opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.Tag))
         )
         .ForMember(dest => dest.Categories,
            opt => opt.MapFrom(src =>
            src.PostCategories.Select(pt => pt.Category)))
        ;
        CreateMap<Post, PostUpsertDto>().ReverseMap();

        CreateMap<Tag, TagReadDto>().ReverseMap();
        CreateMap<Tag, TagUpsertDto>().ReverseMap();

        CreateMap<Category, CategoryReadDto>().ReverseMap();
        CreateMap<Category, CategoryUpsertDto>().ReverseMap();

    }
}