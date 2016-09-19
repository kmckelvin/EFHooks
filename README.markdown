![AppVeyor build status](https://ci.appveyor.com/api/projects/status/g0ipmyws6mc5riew?svg=true)

EFHooks is a framework to assist in hooking into the Entity Framework Code First before and after insert, update and delete actions are performed on the database.

EFHooks is designed to lend itself to code that is easy to unit test with the least amount of mocking possible and without cluttering up your DbContext class with hooking code.  It also is designed to play well with IoC containers.

Getting Started:

Define a hook to fire before an action by deriving from one of the strongly typed hook classes: `PreInsertHook<TEntity>`, `PreUpdateHook<TEntity>` or `PreDeleteHook<TEntity>` and override the `Hook` method. (There are also Post-Action hooks)

The example below will automatically set the `CreatedAt` property to `DateTime.Now`

    public class TimestampPreInsertHook : PreInsertHook<ITimeStamped>
    {
        public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
        {
            entity.CreatedAt = DateTime.Now;
        }
    }

Then derive your DbContext from the EFHooks.HookedDbContext and register the hooks.

    public class AppContext : HookedDbContext
    {
        public AppContext() : base()
        {
            this.RegisterHook(new TimestampPreInsertHook());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
    }

New up the AppContext and your hooks are in place and will fire when you call `SaveChanges();`