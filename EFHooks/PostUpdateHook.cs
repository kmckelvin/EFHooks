using System.Data.Entity;

namespace EFHooks
{
    /// <summary>
    /// Implements a hook that will run after an entity gets updated in the database.
    /// </summary>
    public abstract class PostUpdateHook<TEntity> : PostActionHook<TEntity>
    {
		/// <summary>
		/// Returns <see cref="EntityState.Modified"/> as the hookstate to listen for.
		/// </summary>
        public override EntityState HookStates
        {
            get { return EntityState.Modified; }
        }
    }
}