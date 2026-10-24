# Mecesoft.ObjectMapper

Minimal, reflection-based object mapper for .NET, inspired by AutoMapper.

> **Status:** foundation release (`1.0.0`). The package is not published to NuGet yet.

## Features

- `CreateMap` configuration and reusable `Profile` classes
- `ForMember` + `MapFrom` custom member mapping
- `Ignore` for destination members
- Basic type conversions (numeric, Guid/string, string/number to enum)
- Nested mapping when a map is configured
- Collection mapping for arrays, lists and common generic collection interfaces
- Cycle-safe mapping with per-call reference tracking
- Strict mapping errors with source type, destination type and member path
- Duplicate `CreateMap` detection
- Atomic profile registration and reverse-map rollback on configuration errors
- Optional startup validation through the separate DI package
- Indexer-safe reflection-based member discovery
- Multi-targeting: `net8.0` and `net10.0`

## Quick start

```csharp
using Mecesoft.ObjectMapper;

var configuration = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Customer, CustomerDto>()
        .ForMember(dest => dest.FullName, options =>
            options.MapFrom(src => $"{src.FirstName} {src.LastName}"))
        .Ignore(dest => dest.SecretNote);

    cfg.CreateMap<Address, AddressDto>();
});

var mapper = configuration.CreateMapper();
var dto = mapper.Map<CustomerDto>(
    new Customer { FirstName = "Ada", LastName = "Lovelace" });
```

Profiles group related maps:

```csharp
internal sealed class DemoProfile : Profile
{
    public DemoProfile()
    {
        CreateMap<Customer, CustomerDto>();
    }
}

var configuration = new MapperConfiguration(cfg => cfg.AddProfile(new DemoProfile()));
```

## Dependency injection

DI integration lives in a separate package, so the core package has no
`Microsoft.Extensions.DependencyInjection` dependency:

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

`IMapper` is a singleton, the configuration is created once per provider, and the mapper can
be injected into singleton, scoped and transient services. Options, explicit profiles,
assembly scanning and optional validation are supported by
`Mecesoft.ObjectMapper.DependencyInjection`. See its README for details.

When DI validation is enabled, the configuration is built and validated during
`AddMecesoftObjectMapper` (application startup), not on the first `IMapper` resolution.
This keeps the core package free of ASP.NET Core/hosting dependencies.

## Public API

| Type | Purpose |
| --- | --- |
| `IMapper` | `Map<TDestination>(object)`, `Map<TSource, TDestination>(TSource)`, `Map(object, Type)` |
| `MapperConfiguration` | Builds maps, creates an `IMapper` and provides `AssertConfigurationIsValid()` |
| `MapperConfigurationOptions` | Optional `MaxDepth` and `MaxCollectionSize` configuration |
| `IMapperConfigurationExpression` | `CreateMap<TSource, TDestination>()`, `AddProfile(Profile)` |
| `MappingExpression<TSource, TDestination>` | `ForMember(...)`, `Ignore(...)`, `ReverseMap()` |
| `IMemberOptions<TSource>` | `MapFrom(...)`, `Ignore()` |
| `Profile` | Base class for reusable map definitions |
| `MappingException` | Failed member conversion or nested mapping; exposes `SourceType`, `DestinationType`, `MemberPath` |
| `MappingConfigurationException` | Invalid configuration, including duplicate maps and nested destination paths |

## Mapping behavior

### Reference handling

Every top-level `Map` call gets an isolated reference-tracking context. Within one call:

- Cyclic object and collection graphs are preserved and do not cause unbounded recursion.
- For non-collection objects, a source instance mapped to the same destination type reuses
  the same destination instance, so shared references remain shared.
- A source collection is materialized into a new destination collection. Repeated
  encounters of the same source collection with the same destination type reuse that
  destination collection.
- Tracking is reset between top-level `Map` calls.

A `null` member value is assigned when the destination is a nullable value type or a
reference type. A non-nullable value-type destination keeps its default value.

### Collections

A source collection is any non-string type that implements a single `IEnumerable<T>`.
Dictionaries are not supported. Supported destination shapes are:

| Destination | Result |
| --- | --- |
| `T[]` | New one-dimensional array |
| `List<T>` | New `List<T>` |
| `IEnumerable<T>` | New `List<T>` |
| `ICollection<T>` / `IList<T>` | New `List<T>` |
| `IReadOnlyCollection<T>` / `IReadOnlyList<T>` | New `List<T>` |

Elements are mapped with the configured `CreateMap` for the element types. Primitive,
enum, `Guid` and nullable conversions work element-wise. A new destination collection is
created even when the source and destination element types are identical.

Collection rules:

- A `null` source collection stays `null`.
- An empty source collection produces a new empty destination collection.
- A `null` element stays `null` for reference and nullable element types.
- A `null` element mapped to a non-nullable value type throws `MappingException`.
- Element failures include the collection index in the member path, for example
  `Orders[2].Price`.
- Shared element references and collection references are preserved within one top-level
  `Map` call using the same reference-tracking context as objects.
- Unsupported collection destinations and dictionaries throw `MappingException` instead of
  being assigned silently.

### Collection size limit

Each individual collection is limited to 100,000 elements by default. The limit applies
per collection, including nested collections; the total graph can therefore contain more
than the limit (for example, an outer collection near the limit whose elements each contain
another collection near the limit). It bounds destination allocation and reference-tracking
growth for a single collection without imposing a global object budget.

- Known `Count` values are checked before the destination collection is allocated.
- Lazy/unknown-count sequences are checked element by element; array destinations are
  materialized only up to the limit.
- Exceeding the limit throws `MappingException` with the collection path, the limit and the
  observed/attempted count. For lazy sequences the path includes the failing index.
- Set `MaxCollectionSize = int.MaxValue` to effectively disable the limit.
- The limit is captured when `MapperConfiguration` is constructed.

```csharp
var options = new MapperConfigurationOptions
{
    MaxCollectionSize = 1_000_000
};
```

Collection mapping remains eager. The limit protects against unbounded materialization, but
a very large configured limit can still consume large amounts of memory. Cancellation and
timeouts are not provided.

### Reverse mapping

A reverse map is created only when explicitly requested:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
   .ReverseMap();
```

`ReverseMap` adds a conventional map for `TDestination -> TSource` to the same mapping
configuration. It does not create a second `MapperConfiguration`, and it returns
`MappingExpression<TDestination, TSource>` so reverse-specific rules can be configured:

```csharp
var reverse = cfg.CreateMap<Customer, CustomerDto>()
    .ReverseMap();

reverse.Ignore(dest => dest.SecretNote);
```

Forward `Ignore` and `MapFrom` rules are **not inverted automatically**. The reverse map is
conventional; add rules to the returned expression or define an explicit reverse map when
needed. Nested and collection element types also need their own reverse/explicit maps.

Reverse maps behave like any other map: they participate in strict conversion, nested and
collection mapping, circular/shared-reference tracking, profiles, configuration validation,
configuration snapshots and DI registration.

Duplicate reverse maps and explicit reverse maps are rejected with
`MappingConfigurationException`; an existing explicit reverse map is never overridden.

### Derived source types

If an exact source/destination map does not exist, the mapper looks for a configured map
whose source type is assignable from the runtime source type and whose destination type
matches exactly. An exact map always wins. Among fallback candidates, the most-derived
source type wins; a class map is preferred over an interface map at the same distance.
Ambiguous interface candidates throw `MappingConfigurationException`. Mapping uses the
configured source map's members; derived-only members are ignored unless an explicit
derived map is defined.

### Depth limit

Object and collection mapping levels are limited to 128 by default. Cycle and shared
references are resolved before the depth check. Exceeding the limit throws
`MappingException` with the current member path instead of risking a stack overflow.

```csharp
var options = new MapperConfigurationOptions { MaxDepth = 256 };
var configuration = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Order, OrderDto>();
}, options);
```

The options are captured when `MapperConfiguration` is constructed. Collection mapping is
eager; `MaxDepth` does not limit a flat, infinite `IEnumerable<T>`. `MaxCollectionSize`
provides the per-collection resource bound; use `Take(...)` or another caller-side bound
when you need a stricter or semantic limit.

### Errors

Member mapping is strict. A non-null value that cannot be assigned, converted or mapped
through a configured nested map causes `MappingException` to be thrown. Its message and
properties include the
containing source type, destination type and full member path (for example `Address.City`).
A failing `MapFrom` expression is wrapped the same way and retains the original exception as
`InnerException`.

Source getters, destination setters, destination constructor invocation failures and collection
enumerators are also wrapped with `MappingException`, preserving `SourceType`,
`DestinationType`, `MemberPath` and the inner exception. Collection enumerator failures
include the element index. A missing parameterless destination constructor continues to
throw `MissingMethodException` for compatibility. Fatal CLR exceptions such as
`OutOfMemoryException`, `AccessViolationException` and `InvalidProgramException`, as well as
`OperationCanceledException`, are not wrapped.

Ignored members never participate in mapping and therefore cannot raise conversion errors.
Duplicate `CreateMap` definitions and nested destination member paths such as
`dest => dest.Address.City` are rejected at configuration time with
`MappingConfigurationException`. A missing top-level map continues to throw
`InvalidOperationException`.

Collection element failures use the same exception contract and include the zero-based
element index in `MemberPath` (for example `Orders[2].Price`). Unsupported collection
destinations, dictionaries and collections exceeding `MaxCollectionSize` also throw
`MappingException`. Enumerator `Dispose` failures are wrapped as well; if a mapping or
enumeration error is already in flight, it remains the primary exception and the dispose
error is attached under `Exception.Data["CollectionDisposeException"]`.

### Configuration validation

Validation is opt-in and never runs during `Map`:

```csharp
configuration.AssertConfigurationIsValid();
```

It reports destination members without a source, `Ignore` or `MapFrom`; missing nested or
collection element maps; unsupported collection shapes; invalid type conversions; and
ambiguous base/interface map resolution. Reverse maps participate in validation and are
marked with `(reverse)` in diagnostics. Validation is static, so `MapFrom` return values
and runtime values are not validated. Ignored members and members with `MapFrom` are
treated as intentional.

### Configuration integrity

Configuration registration is transactional:

- A duplicate `CreateMap` does not alter the existing configuration.
- `AddProfile` validates and adds all profile maps as one batch. If any map conflicts, none
  of the profile maps are added.
- `ReverseMap` registers the reverse map and rolls back the forward map created by the same
  expression if registration fails. The failed expression cannot be reused.
- A failed registration leaves the configuration usable for other maps and for
  `CreateMapper`.
- Existing mappers keep their snapshots; rollback and later configuration changes only
  affect mappers created afterwards.

### Configuration snapshot

`CreateMapper` captures an immutable snapshot of the current type maps and member options.
Changes made to `MapperConfiguration` after that point apply only to mappers created later.
Mapping plans are built lazily; configuration-specific plans are cached per mapper, while
conventional plans and compiled accessors are shared process-wide. The caches and `Map`
calls are thread-safe.

## Build and test

The repository includes a local `NuGet.config` that clears machine/global sources and pins
`nuget.org`, so restore is reproducible in CI and does not depend on unrelated package feeds.

```zsh
dotnet restore Mecesoft.ObjectMapper.sln
dotnet build Mecesoft.ObjectMapper.sln --configuration Release
dotnet test Mecesoft.ObjectMapper.sln --configuration Release
```

The core and dependency-injection test projects target both `net8.0` and `net10.0`.
Circular reference behavior is covered by in-process regression tests and a child-process
safety probe, so a future recursion regression cannot terminate the main test run.

## Benchmarks

```zsh
dotnet run --project Mecesoft.ObjectMapper.Benchmarks \
  --configuration Release -- --filter "*"
```

The benchmark project targets `net10.0` and covers flat, nested, collection, custom
`MapFrom`, circular and manual mapping scenarios. Baseline and post-optimization reports
are in `benchmark-results/`.

## Pack

```zsh
dotnet pack Mecesoft.ObjectMapper/Mecesoft.ObjectMapper.csproj \
  --configuration Release \
  --output artifacts

dotnet pack Mecesoft.ObjectMapper.DependencyInjection/Mecesoft.ObjectMapper.DependencyInjection.csproj \
  --configuration Release \
  --output artifacts
```

Both packages include symbol packages (`.snupkg`), SourceLink metadata and deterministic
build settings. Local packing is supported, but publishing is intentionally out of scope for
this phase.

## Demo

```zsh
dotnet run --project Mecesoft.ObjectMapper.Demo/Mecesoft.ObjectMapper.Demo.csproj
```

## AOT, trimming and security

The core mapper is reflection-based. It discovers public properties at runtime and compiles
property accessors with `Expression.Compile`; a reflection fallback is used when compilation
is unavailable. NativeAOT and trimming are **not validated** for the core package. If those
deployment modes are required, validate the application with the target runtime and avoid
relying on conventional reflection discovery.

The DI assembly-scanning API is annotated as unsafe for trimming/NativeAOT and should be
replaced by explicit profile registration. Explicit profiles still use core reflection-based
mapping.

Mapping is convention-based: every writable public destination property with a readable
source property of the same name is considered unless it is ignored or configured. Ignore
sensitive members explicitly. `MapFrom` executes application-supplied delegates without a
sandbox. Collection limits and `MaxDepth` bound individual collections and graph depth, but
nested collections can still increase aggregate memory; there is no cancellation or timeout.

Both packages include XML documentation, README and the MIT license in the NuGet package.

## Known limitations

- Dictionary mapping is not supported.
- Destination collection types are limited to one-dimensional arrays, `List<T>`,
  `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>` and
  `IReadOnlyList<T>`. Other collection destinations throw `MappingException`.
- Source polymorphism accepts assignable base/interface source maps, but destination types
  must match exactly. Ambiguous interface candidates throw `MappingConfigurationException`.
- A base source map ignores derived-only source members unless an explicit derived map is
  defined.
- `ReverseMap` adds one conventional reverse pair at a time; it does not infer or invert
  custom `Ignore`/`MapFrom` rules, and nested types need their own maps.
- Collection mapping is eager; lazy sequences are materialized once per mapping. Infinite
  sequences are bounded by `MaxCollectionSize` (default 100,000 per collection), but there
  is no cancellation or timeout. Nested collections each receive the limit, so aggregate
  memory can exceed a single collection's bound.
- `MaxCollectionSize` defaults to 100,000 per collection; set it higher or to `int.MaxValue`
  through `MapperConfigurationOptions` when a larger trusted collection is expected.
- `MaxDepth` defaults to 128; graphs deeper than that throw `MappingException` unless the
  limit is raised through `MapperConfigurationOptions`.
- Configuration validation is static. It cannot validate `MapFrom` return values or runtime
  data.
- A failed `ReverseMap` rolls back the forward map created by the same expression; the
  expression cannot be reused. Explicit reverse maps are never overridden.
- Nested destination member paths in `ForMember` / `Ignore` are not supported and are
  rejected at configuration time.
- Mapping plans and accessors are cached; property accessors are expression-compiled with
  a reflection fallback. NativeAOT/trimming behavior is not validated and requires further
  annotations/testing. Global caches are process-lifetime and may retain reflection metadata.
- Only writable destination properties and readable source properties are considered.
- Indexer properties are ignored on both the source and destination side.

## License

MIT. See [LICENSE](LICENSE).
