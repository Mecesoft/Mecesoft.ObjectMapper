namespace Mecesoft.ObjectMapper.Demo;

internal sealed class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public Address? Address { get; set; }
}

internal sealed class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public AddressDto? Address { get; set; }
    public string? SecretNote { get; set; }
}

internal sealed class Address
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

internal sealed class AddressDto
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

internal sealed class ConversionSource
{
    public string StatusText { get; set; } = string.Empty;
    public string GuidText { get; set; } = string.Empty;
    public int CodeNumber { get; set; }
}

internal sealed class ConversionDto
{
    public Status Status { get; set; }
    public Guid Guid { get; set; }
    public CodeType Code { get; set; }
}

internal enum Status
{
    Unknown = 0,
    Active = 1,
    Disabled = 2
}

internal enum CodeType
{
    None = 0,
    Basic = 1,
    Premium = 2
}

