EFHooks is a framework to assist in hooking into the Entity Framework Code First before and after insert, update and delete actions are performed on the database.

EFHooks is designed to lend itself to code that is easy to unit test with the least amount of mocking possible and without cluttering up your DbContext class with hooking code.  It also is designed to play well with IoC containers.

Getting Started:

Currently only pre-action hooks are implemented.

Define a hook by deriving from one of the strongly typed hook classes: `PreInsertHook<TEntity>`, `PreUpdateHook<TEntity>` or `PreDeleteHook<TEntity>` and override the `Hook` method.

The example below will automatically set the `CreatedAt` property to `DateTime.Now`

    public class TimestampPreInsertHook : PreInsertHook<ITimeStamped>
    {
        public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
        {
            entity.CreatedAt = DateTime.Now;
        }
    }

Then derive your DbContext from the EFHooks.HookedDbContext and register the hooks.

    private class AppContext : HookedDbContext
    {
        public AppContext() : base()
        {
            this.RegisterHook(new TimestampPreInsertHook());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
    }

New up the AppContext and your hooks are in place and will fire when you call `SaveChanges();`