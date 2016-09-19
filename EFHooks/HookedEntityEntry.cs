using System.Data.Entity;

namespace EFHooks
{
    internal class HookedEntityEntry
    {
        public object Entity { get; set; }
		/// <summary>
		/// Gets or sets the state of the entity before saving.
		/// </summary>
		/// <value>
		/// The state of the entity before saving.
		/// </value>
        public EntityState PreSaveState { get; set; }
    }
}