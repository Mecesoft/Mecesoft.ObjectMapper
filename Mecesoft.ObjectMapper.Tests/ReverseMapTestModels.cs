namespace Mecesoft.ObjectMapper.Tests;

internal sealed class ReverseFlatSource
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Secret { get; set; } = "source-secret";
}

internal sealed class ReverseFlatDestination
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Secret { get; set; } = "destination-secret";
}

internal sealed class ReverseAddressSource
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

internal sealed class ReverseAddressDestination
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

internal sealed class ReverseCustomerSource
{
    public string Name { get; set; } = string.Empty;
    public ReverseAddressSource? Address { get; set; }
}

internal sealed class ReverseCustomerDestination
{
    public string Name { get; set; } = string.Empty;
    public ReverseAddressDestination? Address { get; set; }
}

internal sealed class ReverseItemSource
{
    public int Id { get; set; }
}

internal sealed class ReverseItemDestination
{
    public int Id { get; set; }
}

internal sealed class ReverseOrderSource
{
    public int Id { get; set; }
    public List<ReverseItemSource> Items { get; set; } = new();
}

internal sealed class ReverseOrderDestination
{
    public long Id { get; set; }
    public List<ReverseItemDestination> Items { get; set; } = new();
}

internal sealed class ReverseNodeSource
{
    public string Name { get; set; } = string.Empty;
    public ReverseNodeSource? Next { get; set; }
}

internal sealed class ReverseNodeDestination
{
    public string Name { get; set; } = string.Empty;
    public ReverseNodeDestination? Next { get; set; }
}

internal sealed class ReverseNumberSource
{
    public int Value { get; set; }
}

internal sealed class ReverseNumberDestination
{
    public long Value { get; set; }
}

internal sealed class ReverseMapProfile : Profile
{
    public ReverseMapProfile()
    {
        CreateMap<ReverseFlatSource, ReverseFlatDestination>().ReverseMap();
    }
}
