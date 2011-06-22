using System.Data;

namespace EFHooks
{
    internal class HookedEntityEntry
    {
        public object Entity { get; set; }
        public EntityState PreSaveState { get; set; }
    }
}