namespace Mecesoft.ObjectMapper.Tests;

public sealed class ExceptionReliabilityTests
{
    [Fact]
    public void Source_getter_failure_is_wrapped_with_member_context()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<ThrowingGetterSource, SimpleStringDestination>()).CreateMapper();

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<SimpleStringDestination>(new ThrowingGetterSource()));

        Assert.Equal("Value", exception.MemberPath);
        Assert.Equal(typeof(ThrowingGetterSource), exception.SourceType);
        Assert.Equal(typeof(SimpleStringDestination), exception.DestinationType);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Value", exception.Message);
    }

    [Fact]
    public void Destination_setter_failure_is_wrapped_with_member_context()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<SimpleStringSource, ThrowingSetterDestination>()).CreateMapper();

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<ThrowingSetterDestination>(new SimpleStringSource()));

        Assert.Equal("Value", exception.MemberPath);
        Assert.Equal(typeof(SimpleStringSource), exception.SourceType);
        Assert.Equal(typeof(ThrowingSetterDestination), exception.DestinationType);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void Constructor_failure_is_wrapped_with_member_context()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<SimpleStringSource, ThrowingConstructorDestination>()).CreateMapper();

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<ThrowingConstructorDestination>(new SimpleStringSource()));

        Assert.Equal("$", exception.MemberPath);
        Assert.Equal(typeof(SimpleStringSource), exception.SourceType);
        Assert.Equal(typeof(ThrowingConstructorDestination), exception.DestinationType);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void Collection_enumerator_failure_reports_index()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<EnumerableSource, IntListDestination>()).CreateMapper();

        var source = new EnumerableSource { Values = new ThrowingEnumerable() };

        var exception = Assert.Throws<MappingException>(
            () => mapper.Map<IntListDestination>(source));

        Assert.Equal("Values[1]", exception.MemberPath);
        Assert.Equal(typeof(EnumerableSource), exception.SourceType);
        Assert.Equal(typeof(IntListDestination), exception.DestinationType);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Values[1]", exception.Message);
        Assert.Contains("index 1", exception.Message);
    }

    [Fact]
    public void Fatal_clr_exceptions_are_not_wrapped()
    {
        var mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<FatalGetterSource, SimpleStringDestination>()).CreateMapper();

        Assert.Throws<InvalidProgramException>(
            () => mapper.Map<SimpleStringDestination>(new FatalGetterSource()));
    }
}
