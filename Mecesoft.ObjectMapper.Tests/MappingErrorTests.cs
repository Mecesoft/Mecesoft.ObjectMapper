namespace Mecesoft.ObjectMapper.Tests;

public sealed class MappingErrorTests
{
    [Fact]
    public void Nested_conversion_failure_reports_full_member_path()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ContactSource, ContactDestination>();
            cfg.CreateMap<ContactDetails, ContactDetailsDto>();
        }).CreateMapper();

        var source = new ContactSource
        {
            Details = new ContactDetails
            {
                City = "Istanbul",
                Zip = "not-a-number"
            }
        };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<ContactDestination>(source));

        Assert.Equal("Details.Zip", exception.MemberPath);
        Assert.Equal(typeof(ContactDetails), exception.SourceType);
        Assert.Equal(typeof(ContactDetailsDto), exception.DestinationType);
        Assert.Contains("Details.Zip", exception.Message);
        Assert.Contains(typeof(ContactDetails).FullName!, exception.Message);
        Assert.Contains(typeof(ContactDetailsDto).FullName!, exception.Message);
        Assert.Contains(typeof(int).FullName!, exception.Message);
    }
}
