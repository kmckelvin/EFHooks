using System.Data;

namespace EFHooks
{
    public interface IPostActionHook : IHook
    {
        EntityState HookStates { get; }
    }
}