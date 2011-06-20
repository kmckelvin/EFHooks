using System.Data;

namespace EFHooks
{
    public abstract class PreInsertHook<TEntity> : PreActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Added; }
        }
    }
}