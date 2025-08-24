namespace CVBuilder.Core.Interfaces
{
    public interface ITemplateCatalog
    {
        IReadOnlyDictionary<string, Type> All { get; }
        IEnumerable<string> Names { get; }
        bool TryGet(string name, out Type componentType);
    }
}
