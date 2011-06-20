using System.Data;

namespace EFHooks
{
    public abstract class PreUpdateHook<TEntity> : PreActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Modified; }
        }
    }
}