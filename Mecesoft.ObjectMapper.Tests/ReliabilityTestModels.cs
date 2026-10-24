using System.Collections;

namespace Mecesoft.ObjectMapper.Tests;

internal sealed class ThrowingGetterSource
{
    public string Value => throw new InvalidOperationException("getter boom");
}

internal sealed class SimpleStringSource
{
    public string Value { get; set; } = "value";
}

internal sealed class SimpleStringDestination
{
    public string Value { get; set; } = string.Empty;
}

internal sealed class ThrowingSetterDestination
{
    private string _value = string.Empty;

    public string Value
    {
        get => _value;
        set => throw new InvalidOperationException("setter boom");
    }
}

internal sealed class ThrowingConstructorDestination
{
    public ThrowingConstructorDestination()
    {
        throw new InvalidOperationException("constructor boom");
    }

    public string Value { get; set; } = string.Empty;
}

internal sealed class FatalGetterSource
{
    public string Value => throw new InvalidProgramException("fatal boom");
}

internal sealed class ThrowingEnumerable : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        return new Enumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class Enumerator : IEnumerator<int>
    {
        private int _index;

        public int Current => _index;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;

            if (_index == 1)
            {
                return true;
            }

            throw new InvalidOperationException("enumerator boom");
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}

internal sealed class EnumerableSource
{
    public IEnumerable<int> Values { get; set; } = Array.Empty<int>();
}

internal sealed class IntListDestination
{
    public List<int> Values { get; set; } = new();
}

internal class AnimalSource
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class DogSource : AnimalSource
{
    public int BarkVolume { get; set; }
}

internal sealed class CatSource : AnimalSource
{
    public int Lives { get; set; }
}

internal class AnimalDto
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class DogDto : AnimalDto
{
    public int BarkVolume { get; set; }
}

internal sealed class ZooSource
{
    public List<AnimalSource> Animals { get; set; } = new();
}

internal sealed class ZooDto
{
    public List<AnimalDto> Animals { get; set; } = new();
}

internal class LevelASource
{
    public string Name { get; set; } = string.Empty;
}

internal class LevelBSource : LevelASource
{
    public string BValue { get; set; } = "b";
}

internal sealed class LevelCSource : LevelBSource
{
    public string CValue { get; set; } = "c";
}

internal sealed class LevelDto
{
    public string Name { get; set; } = string.Empty;
    public string BValue { get; set; } = "unset";
    public string CValue { get; set; } = "unset";
    public string Source { get; set; } = string.Empty;
}

internal interface ILeftSource
{
    string Name { get; }
}

internal interface IRightSource
{
    string Name { get; }
}

internal sealed class MultiSource : ILeftSource, IRightSource
{
    public string Name { get; set; } = "multi";
}

internal class BaseTaggableSource
{
    public string Name { get; set; } = string.Empty;
}

internal interface ITaggedSource
{
    string Name { get; }
}

internal sealed class TaggedDerivedSource : BaseTaggableSource, ITaggedSource
{
    public string Tag { get; set; } = "tag";
}

internal sealed class TaggedDto
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

internal sealed class ValidationChildSource
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class ValidationChildDestination
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class ValidationParentSource
{
    public ValidationChildSource Child { get; set; } = new();
}

internal sealed class ValidationParentDestination
{
    public ValidationChildDestination Child { get; set; } = new();
}

internal sealed class ValidationCollectionSource
{
    public List<ValidationChildSource> Children { get; set; } = new();
}

internal sealed class ValidationCollectionDestination
{
    public List<ValidationChildDestination> Children { get; set; } = new();
}

internal sealed class ValidationUnmappedSource
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class ValidationUnmappedDestination
{
    public string Name { get; set; } = string.Empty;
    public string Optional { get; set; } = "optional";
}

internal sealed class ValidationBadSource
{
    public Guid Value { get; set; }
}

internal sealed class ValidationBadDestination
{
    public int Value { get; set; }
}

internal sealed class ValidationDerivedSource
{
    public DogSource Dog { get; set; } = new();
}

internal sealed class ValidationDerivedDestination
{
    public AnimalDto Dog { get; set; } = new();
}

internal sealed class ValidationAmbiguousSource
{
    public MultiSource Value { get; set; } = new();
}

internal sealed class ValidationAmbiguousDestination
{
    public TaggedDto Value { get; set; } = new();
}

internal sealed class DeepNodeSource
{
    public string Name { get; set; } = string.Empty;
    public DeepNodeSource? Next { get; set; }
}

internal sealed class DeepNodeDto
{
    public string Name { get; set; } = string.Empty;
    public DeepNodeDto? Next { get; set; }
}
