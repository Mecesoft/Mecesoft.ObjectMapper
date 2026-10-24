# Mecesoft.ObjectMapper.DependencyInjection

`Microsoft.Extensions.DependencyInjection` integration for `Mecesoft.ObjectMapper`.

> **Status:** `1.0.0`. Not published to NuGet yet.

## Installation

Reference `Mecesoft.ObjectMapper.DependencyInjection`. The core
`Mecesoft.ObjectMapper` package remains free of DI dependencies and is brought in
transitively by this package.

## Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Mecesoft.ObjectMapper;

var services = new ServiceCollection();

services.AddMecesoftObjectMapper(cfg =>
{
    cfg.CreateMap<Customer, CustomerDto>();
});

using var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IMapper>();
```

- `IMapper` is registered as a **singleton**.
- By default, the configuration is created once when the mapper is first resolved.
- When `AssertConfigurationIsValid` is enabled, the configuration is built and validated
  during `AddMecesoftObjectMapper` (application startup) instead.
- The mapper can be injected into singleton, scoped or transient services.
- `ReverseMap()` is available in the configuration action:

```csharp
services.AddMecesoftObjectMapper(cfg =>
    cfg.CreateMap<Customer, CustomerDto>().ReverseMap());
```

## Options

```csharp
services.AddMecesoftObjectMapper(
    cfg => cfg.CreateMap<Customer, CustomerDto>(),
    new MapperConfigurationOptions
    {
        MaxDepth = 256,
        MaxCollectionSize = 50_000
    });
```

Options are captured when `AddMecesoftObjectMapper` is called.

## Profiles, scanning and validation

```csharp
using Mecesoft.ObjectMapper.DependencyInjection;

services.AddMecesoftObjectMapper(
    cfg => cfg.CreateMap<Customer, CustomerDto>(),
    options =>
    {
        options.AddProfile<CustomerProfile>();
        options.AddProfile(new OrderProfile());
        options.AssertConfigurationIsValid = true;

        options.Configuration.MaxDepth = 256;
        options.Configuration.MaxCollectionSize = 50_000;
    });
```

Assembly scanning is an explicit opt-in:

```csharp
services.AddMecesoftObjectMapper(
    cfg => cfg.CreateMap<Customer, CustomerDto>(),
    options => options.AddProfilesFromAssembly(typeof(CustomerProfile).Assembly));
```

- Explicit profiles are applied in registration order.
- Profile scanning discovers concrete `Profile` types with a public parameterless
  constructor and sorts them by full name using ordinal comparison.
- Duplicate maps are rejected by the core library when the configuration is built.
- `AssertConfigurationIsValid` is opt-in. When enabled, it builds and validates the
  configuration during `AddMecesoftObjectMapper`; invalid configuration throws at startup,
  before the service provider is built and before the mapper is first resolved.
- Startup validation failure leaves the `IServiceCollection` without the mapper registration.
  Configuration actions may still have observed user-side side effects because they run
  during registration.
- Reverse maps created by `ReverseMap()` are included in validation and missing reverse
  nested/collection maps are reported with a `(reverse)` marker.
- Assembly scanning uses reflection and is **not NativeAOT/trimming safe**; prefer explicit
  profile registration for trimmed or NativeAOT applications. Core mapping remains
  reflection-based even with explicit profiles.

## Startup validation

The DI package does not depend on ASP.NET Core hosting abstractions. Validation is performed
at registration, which is the startup phase for a typical application:

```csharp
services.AddMecesoftObjectMapper(
    cfg => cfg.CreateMap<Customer, CustomerDto>().ReverseMap(),
    options => options.AssertConfigurationIsValid = true);
```

This makes an invalid configuration fail while the application is configuring services,
rather than on the first request that resolves `IMapper`. It does not require
`IHostedService` or a framework-specific integration package.

## Duplicate and existing registrations

`AddMecesoftObjectMapper` is intentionally non-overriding:

- A second call on the same `IServiceCollection` throws `InvalidOperationException`.
- If an `IMapper` service is already registered by another component, registration throws
  `InvalidOperationException` instead of replacing it.

Build a separate service collection if different mapper configurations are required.

## Configuration snapshot

The type maps and member options are snapshotted by `CreateMapper`. With startup validation,
the singleton mapper and snapshot are created during registration; otherwise it is created
on first resolution. Changes to the configuration after mapper creation do not affect that
mapper. The DI registration options are frozen when `AddMecesoftObjectMapper` is called.

## AOT and trimming

The DI package is not a NativeAOT/trimming guarantee. `AddProfilesFromAssembly` is annotated
with `[RequiresUnreferencedCode]` because it discovers and constructs profile types by
reflection. Prefer `AddProfile`/`AddProfile<TProfile>()` in trimmed or NativeAOT builds; core
mapping still discovers and compiles property accessors reflectively.

## License

MIT. See [LICENSE](../LICENSE).
