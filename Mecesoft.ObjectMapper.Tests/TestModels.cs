namespace Mecesoft.ObjectMapper.Tests;

internal sealed class BasicSource
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Secret { get; set; } = "source-secret";
    public int? OptionalNumber { get; set; }
    public int? NullableNumber { get; set; }
    public int? NullableNumberNullable { get; set; }
    public string? NullableText { get; set; }
    public string ReadOnly => "source-read-only";
    public Address? Address { get; set; }
    public Address? UnmappedAddress { get; set; }
}

internal sealed class BasicDestination
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Secret { get; set; } = "destination-secret";
    public int? OptionalNumber { get; set; }
    public int NullableNumber { get; set; } = 42;
    public int? NullableNumberNullable { get; set; }
    public string? NullableText { get; set; } = "keep";
    public string DestinationOnly { get; set; } = "unchanged";
    public string ReadOnly { get; } = "read-only";
    public AddressDto? Address { get; set; }
    public AddressDto? UnmappedAddress { get; set; }
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

internal sealed class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; } = new();
}

internal sealed class Customer
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class OrderDto
{
    public int Id { get; set; }
    public CustomerDto? Customer { get; set; }
}

internal sealed class CustomerDto
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class ConversionSource
{
    public int Number { get; set; }
    public string NumberText { get; set; } = "0";
    public string StatusText { get; set; } = "Active";
    public int StatusNumber { get; set; }
    public string GuidText { get; set; } = Guid.Empty.ToString();
    public string InvalidGuidText { get; set; } = Guid.Empty.ToString();
    public long LargeNumber { get; set; }
    public string InvalidNumberText { get; set; } = "0";
    public int? OptionalNumber { get; set; }
}

internal sealed class ConversionDestination
{
    public long Number { get; set; }
    public int NumberText { get; set; }
    public Status Status { get; set; }
    public CodeType Code { get; set; }
    public Guid Guid { get; set; }
    public Guid InvalidGuid { get; set; }
    public int NarrowedNumber { get; set; }
    public int InvalidNumber { get; set; }
    public int? OptionalNumber { get; set; }
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

internal sealed class CircularNode
{
    public string Name { get; set; } = string.Empty;
    public CircularNode? Next { get; set; }
}

internal sealed class CircularNodeDto
{
    public string Name { get; set; } = string.Empty;
    public CircularNodeDto? Next { get; set; }
}

internal sealed class NoDefaultConstructorDestination
{
    public NoDefaultConstructorDestination(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
}

internal sealed class NoMapDestination
{
    public int Id { get; set; }
}

internal class Animal
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class Dog : Animal
{
    public int BarkVolume { get; set; }
}

internal sealed class ProfileSource
{
    public int Value { get; set; }
}

internal sealed class ProfileDestination
{
    public int Value { get; set; }
}

internal sealed class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<ProfileSource, ProfileDestination>();
    }
}

internal sealed class DuplicateMapProfile : Profile
{
    public DuplicateMapProfile()
    {
        CreateMap<ProfileSource, ProfileDestination>();
        CreateMap<ProfileSource, ProfileDestination>();
    }
}

internal sealed class ContactSource
{
    public ContactDetails Details { get; set; } = new();
}

internal sealed class ContactDestination
{
    public string City { get; set; } = "unchanged";
    public ContactDetailsDto Details { get; set; } = new();
}

internal sealed class ContactDetails
{
    public string City { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
}

internal sealed class ContactDetailsDto
{
    public string City { get; set; } = string.Empty;
    public int Zip { get; set; }
}

internal sealed class SharedNode
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class SharedNodeDto
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class HolderSource
{
    public SharedNode Left { get; set; } = new();
    public SharedNode Right { get; set; } = new();
}

internal sealed class HolderDestination
{
    public SharedNodeDto? Left { get; set; }
    public SharedNodeDto? Right { get; set; }
}

internal sealed class IndexerSource
{
    public int Id { get; set; }

    public string this[int index] => $"source-{index}";
}

internal sealed class IndexerDestination
{
    public int Id { get; set; }

    public string StoredValue { get; private set; } = "initial";

    public string this[int index]
    {
        get => StoredValue;
        set => StoredValue = value;
    }
}

internal sealed class IndexerOnlySource
{
    public string this[int index] => $"source-{index}";
}

internal sealed class NamedItemDestination
{
    public string Item { get; set; } = "default";
}

internal sealed class IgnoredFailureSource
{
    public string Value { get; set; } = "not-an-int";
}

internal sealed class IgnoredFailureDestination
{
    public int Value { get; set; } = 7;
}

internal sealed class CollectionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Address? Address { get; set; }
}

internal sealed class CollectionItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public AddressDto? Address { get; set; }
}

internal sealed class CollectionSource
{
    public List<CollectionItem> Items { get; set; } = new();
    public CollectionItem[] ArrayItems { get; set; } = Array.Empty<CollectionItem>();
    public IEnumerable<CollectionItem> EnumerableItems { get; set; } = Array.Empty<CollectionItem>();
    public ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();
    public IList<CollectionItem> ListItems { get; set; } = new List<CollectionItem>();
    public IReadOnlyCollection<CollectionItem> ReadOnlyCollectionItems { get; set; } = Array.Empty<CollectionItem>();
    public IReadOnlyList<CollectionItem> ReadOnlyListItems { get; set; } = new List<CollectionItem>();
}

internal sealed class CollectionDestination
{
    public List<CollectionItemDto> Items { get; set; } = new();
    public CollectionItemDto[] ArrayItems { get; set; } = Array.Empty<CollectionItemDto>();
    public List<CollectionItemDto> EnumerableItems { get; set; } = new();
    public ICollection<CollectionItemDto> CollectionItems { get; set; } = new List<CollectionItemDto>();
    public IList<CollectionItemDto> ListItems { get; set; } = new List<CollectionItemDto>();
    public IReadOnlyCollection<CollectionItemDto> ReadOnlyCollectionItems { get; set; } = new List<CollectionItemDto>();
    public IReadOnlyList<CollectionItemDto> ReadOnlyListItems { get; set; } = new List<CollectionItemDto>();
}

internal sealed class PrimitiveCollectionsSource
{
    public List<int> Numbers { get; set; } = new();
    public List<string> Guids { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
    public List<int?> OptionalNumbers { get; set; } = new();
    public List<int> StatusNumbers { get; set; } = new();
}

internal sealed class PrimitiveCollectionsDestination
{
    public List<long> Numbers { get; set; } = new();
    public List<Guid> Guids { get; set; } = new();
    public List<Status> Statuses { get; set; } = new();
    public List<long?> OptionalNumbers { get; set; } = new();
    public List<CodeType> StatusNumbers { get; set; } = new();
}

internal sealed class NullableElementSource
{
    public List<string?> Values { get; set; } = new();
}

internal sealed class NullableElementDestination
{
    public List<int?> Values { get; set; } = new();
}

internal sealed class NonNullableElementDestination
{
    public List<int> Values { get; set; } = new();
}

internal sealed class NullableItemSource
{
    public List<CollectionItem?> Items { get; set; } = new();
}

internal sealed class NullableItemDestination
{
    public List<CollectionItemDto?> Items { get; set; } = new();
}

internal sealed class MissingElementSource
{
    public List<UnmappedThing> Items { get; set; } = new();
}

internal sealed class MissingElementDestination
{
    public List<UnmappedThingDto> Items { get; set; } = new();
}

internal sealed class UnmappedThing
{
    public int Id { get; set; }
}

internal sealed class UnmappedThingDto
{
    public int Id { get; set; }
}

internal sealed class IndexedLineSource
{
    public string Price { get; set; } = string.Empty;
}

internal sealed class IndexedLineDestination
{
    public int Price { get; set; }
}

internal sealed class IndexedOrderSource
{
    public List<IndexedLineSource> Orders { get; set; } = new();
}

internal sealed class IndexedOrderDestination
{
    public List<IndexedLineDestination> Orders { get; set; } = new();
}

internal sealed class Category
{
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

internal sealed class Product
{
    public string Name { get; set; } = string.Empty;
    public Category? Category { get; set; }
}

internal sealed class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new();
}

internal sealed class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public CategoryDto? Category { get; set; }
}

internal sealed class TreeNode
{
    public string Name { get; set; } = string.Empty;
    public List<TreeNode> Children { get; set; } = new();
}

internal sealed class TreeNodeDto
{
    public string Name { get; set; } = string.Empty;
    public List<TreeNodeDto> Children { get; set; } = new();
}

internal sealed class SharedCollectionHolderSource
{
    public List<CollectionItem> First { get; set; } = new();
    public List<CollectionItem> Second { get; set; } = new();
}

internal sealed class SharedCollectionHolderDestination
{
    public List<CollectionItemDto> First { get; set; } = new();
    public List<CollectionItemDto> Second { get; set; } = new();
}

internal sealed class MixedSharedCollectionSource
{
    public List<CollectionItem> AsList { get; set; } = new();
    public List<CollectionItem> AsReadOnly { get; set; } = new();
}

internal sealed class MixedSharedCollectionDestination
{
    public List<CollectionItemDto> AsList { get; set; } = new();
    public IReadOnlyList<CollectionItemDto> AsReadOnly { get; set; } = new List<CollectionItemDto>();
}

internal sealed class SharedAcrossPropertiesSource
{
    public CollectionItem Single { get; set; } = new();
    public List<CollectionItem> Items { get; set; } = new();
}

internal sealed class SharedAcrossPropertiesDestination
{
    public CollectionItemDto? Single { get; set; }
    public List<CollectionItemDto> Items { get; set; } = new();
}

internal sealed class CatalogSource
{
    public CatalogDetails Details { get; set; } = new();
}

internal sealed class CatalogDetails
{
    public List<CollectionItem> Items { get; set; } = new();
}

internal sealed class CatalogDestination
{
    public CatalogDetailsDto Details { get; set; } = new();
}

internal sealed class CatalogDetailsDto
{
    public List<CollectionItemDto> Items { get; set; } = new();
}

internal sealed class UnsupportedCollectionSource
{
    public List<CollectionItem> Items { get; set; } = new();
}

internal sealed class UnsupportedCollectionDestination
{
    public HashSet<CollectionItemDto> Items { get; set; } = new();
}

internal sealed class DictionaryCollectionSource
{
    public Dictionary<string, int> Values { get; set; } = new();
}

internal sealed class DictionaryCollectionDestination
{
    public Dictionary<string, string> Values { get; set; } = new();
}

internal sealed class NonGenericCollectionSource
{
    public System.Collections.ArrayList Values { get; set; } = new();
}

internal sealed class NonGenericCollectionDestination
{
    public List<int> Values { get; set; } = new();
}
