namespace Mecesoft.ObjectMapper.Benchmarks;

public sealed class FlatSource
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid ExternalId { get; set; }
}

public sealed class FlatDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid ExternalId { get; set; }
}

public sealed class AddressSource
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class AddressDto
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class NestedSource
{
    public FlatSource Customer { get; set; } = new();
    public AddressSource Address { get; set; } = new();
}

public sealed class NestedDto
{
    public FlatDto Customer { get; set; } = new();
    public AddressDto Address { get; set; } = new();
}

public sealed class LineSource
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public sealed class LineDto
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public sealed class CollectionSource
{
    public List<LineSource> Items { get; set; } = new();
}

public sealed class CollectionDto
{
    public List<LineDto> Items { get; set; } = new();
}

public sealed class CustomSource
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}

public sealed class CustomDto
{
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public int Age { get; set; }
}

public sealed class NodeSource
{
    public string Name { get; set; } = string.Empty;
    public NodeSource? Next { get; set; }
}

public sealed class NodeDto
{
    public string Name { get; set; } = string.Empty;
    public NodeDto? Next { get; set; }
}
