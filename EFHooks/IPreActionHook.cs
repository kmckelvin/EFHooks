using System.Data;

namespace EFHooks
{
    public interface IPreActionHook: IHook
    {
        EntityState HookStates { get; }
    }
}