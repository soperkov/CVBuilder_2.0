using System.Reflection;

namespace CVBuilder.Api.Services
{
    public class TemplateCatalog : ITemplateCatalog
    {
        private readonly Dictionary<string, Type> _map;

        public TemplateCatalog(Assembly assemblyWithTemplates, string templatesNamespace)
        {
            _map = assemblyWithTemplates
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    typeof(IComponent).IsAssignableFrom(t) &&
                    string.Equals(t.Namespace, templatesNamespace, StringComparison.Ordinal))
                .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<string, Type> All => _map;
        public IEnumerable<string> Names => _map.Keys;
        public bool TryGet(string name, out Type componentType) => _map.TryGetValue(name, out componentType);
    }
}
