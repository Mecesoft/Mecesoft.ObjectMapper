# Mecesoft.ObjectMapper 1.0.0 — Release Notes Draft

Status: draft. Not published to NuGet or GitHub yet.

## Packages

- `Mecesoft.ObjectMapper` 1.0.0
- `Mecesoft.ObjectMapper.DependencyInjection` 1.0.0

Target frameworks: `net8.0` and `net10.0`.

## Highlights

- Conventional object mapping with `CreateMap`, `ForMember`, `MapFrom` and `Ignore`.
- Nested object and collection mapping for arrays, lists and common generic collection interfaces.
- Cycle-safe mapping with per-call shared-reference tracking.
- Strict mapping errors with source type, destination type and member path.
- Explicit `ReverseMap()` support with conventional reverse mapping. Forward `Ignore` and
  `MapFrom` rules are not inverted automatically; configure the returned reverse expression
  or define an explicit reverse map.
- Atomic configuration registration:
  - Failed `ReverseMap` operations roll back the pair created by the expression.
  - Failed `AddProfile` operations add no profile maps.
  - Duplicate maps do not alter the existing configuration.
- Configuration validation for missing members, nested maps, collection element maps,
  invalid conversions and ambiguous polymorphism. Reverse maps are marked with `(reverse)`.
- `MapperConfigurationOptions` limits: `MaxDepth` (default 128) and `MaxCollectionSize`
  (default 100,000 per collection).
- Separate DI package with singleton `IMapper`, profiles, assembly scanning and opt-in
  startup validation. The core package remains dependency-free.
- XML documentation, README and MIT license included in both packages.

## Behavior and compatibility

- Configuration is snapshotted by `CreateMapper`; existing mappers are not changed by later
  configuration mutations.
- DI startup validation runs during registration when `AssertConfigurationIsValid` is true.
  This is earlier than the previous first-resolution behavior.
- A failed `ReverseMap` rolls back the forward map created by the same expression. The failed
  expression cannot be reused.
- Reverse maps are conventional one-pair maps. Nested and collection element types require
  their own maps.

## Known limitations

- Dictionary mapping is not supported.
- Destination collections are limited to one-dimensional arrays, `List<T>`, `IEnumerable<T>`,
  `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>` and `IReadOnlyList<T>`.
- ReverseMap does not infer or invert arbitrary `Ignore`/`MapFrom` rules.
- Validation is static; it cannot validate `MapFrom` return values or runtime semantics.
- Collection mapping is eager. Nested collections each receive `MaxCollectionSize`; aggregate
  memory is not globally bounded and there is no cancellation or timeout.
- NativeAOT/trimming is not validated. Core mapping uses reflection and expression
  compilation; DI assembly scanning is annotated as unreferenced-code dependent.

## Verification

- Core tests: 160.
- DI tests: 20.
- Both test projects run on `net8.0` and `net10.0`.
- Release build, local pack, consumer smoke tests and deterministic build checks are part of
  the release checklist.

## Publication checklist

- Add the GitHub remote and create the first commit on `main`.
- Verify SourceLink output with repository metadata.
- Finish branch protection, code scanning, secret scanning and dependency update settings.
- Add NuGet signing/publishing credentials through a protected release environment, not CI
  pull-request workflows.
- Re-run clean build/test/pack against the final commit.
