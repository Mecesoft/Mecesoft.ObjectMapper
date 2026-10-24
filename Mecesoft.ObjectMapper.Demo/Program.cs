using Mecesoft.ObjectMapper;
using Mecesoft.ObjectMapper.Demo;

var configuration = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new DemoProfile());
});

var mapper = configuration.CreateMapper();

var customer = new Customer
{
    Id = 7,
    FirstName = "Ada",
    LastName = "Lovelace",
    Age = 28,
    Address = new Address { City = "Istanbul", Street = "Istiklal" }
};

var dto = mapper.Map<CustomerDto>(customer);

var conversionSource = new ConversionSource
{
    StatusText = "Active",
    GuidText = "2f4a6c70-8a76-4d0c-9db6-6edb3c8f0e7b",
    CodeNumber = 2
};

var conversionDto = mapper.Map<ConversionDto>(conversionSource);

Console.WriteLine($"Id: {dto.Id}");
Console.WriteLine($"FullName: {dto.FullName}");
Console.WriteLine($"Age: {dto.Age}");
Console.WriteLine($"Address: {dto.Address?.City} - {dto.Address?.Street}");
Console.WriteLine($"SecretNote: {dto.SecretNote ?? "<null>"}");

Console.WriteLine($"Status: {conversionDto.Status}");
Console.WriteLine($"Guid: {conversionDto.Guid}");
Console.WriteLine($"Code: {conversionDto.Code}");
