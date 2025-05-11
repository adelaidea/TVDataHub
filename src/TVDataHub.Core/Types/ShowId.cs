namespace TVDataHub.Core.Types;

public readonly record struct TVShowId(int Value)
{
    public override string ToString() => $"TVShowId: {Value}";
}