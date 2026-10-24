using Mecesoft.ObjectMapper;

namespace Mecesoft.ObjectMapper.Demo;

internal sealed class DemoProfile : Profile
{
    public DemoProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, options =>
                options.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .Ignore(dest => dest.SecretNote);

        CreateMap<Address, AddressDto>();

        CreateMap<ConversionSource, ConversionDto>()
            .ForMember(dest => dest.Status, options => options.MapFrom(src => src.StatusText))
            .ForMember(dest => dest.Guid, options => options.MapFrom(src => src.GuidText))
            .ForMember(dest => dest.Code, options => options.MapFrom(src => src.CodeNumber));
    }
}
