using System.Data;

namespace EFHooks
{
    public abstract class PostActionHook<TEntity> : IPostActionHook
    {
        public void Hook(object entity, HookEntityMetadata metadata)
        {
            if (typeof(TEntity).IsAssignableFrom(entity.GetType()))
            {
                Hook((TEntity) entity, metadata);
            }
        }

        public abstract void Hook(TEntity entity, HookEntityMetadata metadata);

        public abstract EntityState HookStates { get; }
    }
}