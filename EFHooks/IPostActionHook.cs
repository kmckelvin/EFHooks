using System.Data;

namespace EFHooks
{
    interface IPostActionHook : IHook
    {
        EntityState HookStates { get; }
    }
}