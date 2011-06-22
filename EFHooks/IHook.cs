namespace EFHooks
{
    public interface IHook
    {
        void HookObject(object entity, HookEntityMetadata metadata);
    }
}