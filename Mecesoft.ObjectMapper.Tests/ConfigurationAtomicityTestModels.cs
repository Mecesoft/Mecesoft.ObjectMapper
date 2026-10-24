namespace Mecesoft.ObjectMapper.Tests;

internal sealed class AtomicitySource
{
    public int Value { get; set; }
}

internal sealed class AtomicityDestination
{
    public int Value { get; set; }
}

internal sealed class AtomicityProfile : Profile
{
    public AtomicityProfile()
    {
        CreateMap<AtomicitySource, AtomicityDestination>();
        CreateMap<ProfileSource, ProfileDestination>();
    }
}
