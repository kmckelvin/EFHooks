using System.Data;

namespace EFHooks
{
    /// <summary>
    /// A strongly typed PreActionHook.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class PreActionHook<TEntity> : IPreActionHook
    {
        /// <summary>
        /// Entity States that this hook must be registered to listen for.
        /// </summary>
        public abstract EntityState HookStates { get; }

        /// <summary>
        /// The logic to perform per entity before the registered action gets performed.
        /// This gets run once per entity that has been changed.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metadata"></param>
        public abstract void Hook(TEntity entity, HookEntityMetadata metadata);

        /// <summary>
        /// Implements the interface.  This causes the hook to only run for objects that are assignable to TEntity.
        /// </summary>
        public void HookObject(object entity, HookEntityMetadata metadata)
        {
            if (typeof(TEntity).IsAssignableFrom(entity.GetType()))
            {
                this.Hook((TEntity)entity, metadata);
            }
        }
    }
}