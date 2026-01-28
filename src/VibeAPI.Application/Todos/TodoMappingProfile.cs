using AutoMapper;

namespace VibeAPI.Application.Todos;

public sealed class TodoMappingProfile : Profile
{
    public TodoMappingProfile()
    {
        CreateMap<Entities.Todo, VibeAPI.Todos.Todo>()
            .ForCtorParam("CreatedAt", o => o.MapFrom(s => new DateTimeOffset(s.CreatedAt)))
            .ForCtorParam("UpdatedAt", o => o.MapFrom(s => new DateTimeOffset(s.UpdatedAt)));
    }
}
