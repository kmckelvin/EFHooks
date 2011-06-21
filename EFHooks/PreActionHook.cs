using System.Data;

namespace EFHooks
{
    public abstract class PreActionHook<TEntity> : IPreActionHook
    {
        public abstract EntityState HookStates { get; }

        public abstract void Hook(TEntity entity, HookEntityMetadata metadata);

        public void Hook(object entity, HookEntityMetadata metadata)
        {
            if (typeof(TEntity).IsAssignableFrom(entity.GetType()))
            {
                this.Hook((TEntity)entity, metadata);
            }
        }
    }
}