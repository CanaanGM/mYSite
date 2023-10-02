using Application.Dtos;
using DataAccess.Contexts;

namespace Application.Persistence;

public class PostService 
{
    private readonly BlogContext _context;

    public PostService(BlogContext context  )
    {
        _context = context;
        
    }

    //get one 

}