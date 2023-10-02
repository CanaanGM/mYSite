using AutoMapper;
using Domain.Entities;

namespace DataAccess.Dtos;
public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Post, PostReadDto>().ReverseMap();
        CreateMap<Post, PostUpsertDto>().ReverseMap();
    }
}