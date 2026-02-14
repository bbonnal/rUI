namespace rUI.AppModel.Contracts;

public readonly record struct PersistenceKey(string Value)
{
    public override string ToString() => Value;
}
