namespace EFHooks
{
    public interface IHook
    {
        void Hook(object entity, HookEntityMetadata metadata);
    }
}