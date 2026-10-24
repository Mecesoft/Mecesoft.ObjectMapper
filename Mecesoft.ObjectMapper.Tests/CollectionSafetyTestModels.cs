using System.Collections;

namespace Mecesoft.ObjectMapper.Tests;

internal sealed class IntArraySource
{
    public int[] Values { get; set; } = Array.Empty<int>();
}

internal sealed class IntArrayDestination
{
    public int[] Values { get; set; } = Array.Empty<int>();
}

internal sealed class CollectionParentSource
{
    public List<CollectionChildSource> Parents { get; set; } = new();
}

internal sealed class CollectionChildSource
{
    public List<int> Children { get; set; } = new();
}

internal sealed class CollectionParentDestination
{
    public List<CollectionChildDestination> Parents { get; set; } = new();
}

internal sealed class CollectionChildDestination
{
    public List<int> Children { get; set; } = new();
}

internal sealed class DisposeThrowingEnumerable : IEnumerable<int>
{
    private readonly bool _throwDuringEnumeration;

    public DisposeThrowingEnumerable(bool throwDuringEnumeration)
    {
        _throwDuringEnumeration = throwDuringEnumeration;
    }

    public IEnumerator<int> GetEnumerator()
    {
        return new Enumerator(_throwDuringEnumeration);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class Enumerator : IEnumerator<int>
    {
        private readonly bool _throwDuringEnumeration;
        private int _index;

        public Enumerator(bool throwDuringEnumeration)
        {
            _throwDuringEnumeration = throwDuringEnumeration;
        }

        public int Current => 1;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;

            if (_index == 1)
            {
                return true;
            }

            if (_throwDuringEnumeration)
            {
                throw new InvalidOperationException("move next boom");
            }

            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            throw new InvalidOperationException("dispose boom");
        }
    }
}

internal sealed class FatalDisposeEnumerable : IEnumerable<int>
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

        public int Current => 1;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index == 1;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            throw new InvalidProgramException("fatal dispose boom");
        }
    }
}
