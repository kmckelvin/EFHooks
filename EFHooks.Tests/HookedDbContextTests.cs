using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using EFHooks.Tests.Hooks;
using FakeItEasy;
using NUnit.Framework;

namespace EFHooks.Tests
{
    public class HookedDbContextTests
    {
        [SetUp]
        public void Init()
        {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
        }

        private class TimestampPreInsertHook : PreInsertHook<ITimeStamped>
        {
            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.CreatedAt = DateTime.Now;
            }
        }

        private class TimestampPreUpdateHook : PreUpdateHook<ITimeStamped>
        {
            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.ModifiedAt = DateTime.Now;
            }
        }

        private class TimestampPostInsertHook : PostInsertHook<ITimeStamped>
        {
            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.ModifiedAt = DateTime.Now;
            }
        }

        private class RunCheckedPreInsertHook : PreInsertHook<object>
        {
            public bool HasRun { get; set; }
            public override void Hook(object entity, HookEntityMetadata metadata)
            {
                HasRun = true;
            }
        }

        private class LocalContext : HookedDbContext
        {
            public LocalContext() : base()
            {
                
            }

            public LocalContext(IHook[] hooks) : base(hooks)
            {
                
            }

            public DbSet<TimestampedSoftDeletedEntity> Entities { get; set; }
            public DbSet<ValidatedEntity> ValidatedEntities { get; set; }
        }

        private class LocalContextWithNameOrConnectionString : HookedDbContext
        {
            public LocalContextWithNameOrConnectionString()
                : base("EFHooksDatabase")
            {

            }

            public LocalContextWithNameOrConnectionString(IHook[] hooks)
                : base(hooks, "EFHooksDatabase")
            {

            }

            public DbSet<TimestampedSoftDeletedEntity> Entities { get; set; }
            public DbSet<ValidatedEntity> ValidatedEntities { get; set; }
        }

        [Test]
        public void HookedDbContext_ConstructsWithHooks()
        {
            var hooks = new IHook[]
                            {
                                new TimestampPreInsertHook()
                            };

            var context = new LocalContext(hooks);
        }

        [Test]
        public void HookedDbContext_MustNotCallHooks_WhenGetValidationErrorsIsCalled()
        {
            var hooks = new IHook[]
                            {
                                new TimestampPreInsertHook()
                            };

            var context = new LocalContext(hooks);
            var entity = new TimestampedSoftDeletedEntity();
            context.Entities.Add(entity);
            context.GetValidationErrors();

            Assert.AreNotEqual(entity.CreatedAt.Date, DateTime.Today);
        }

        [Test]
        public void HookedDbContext_MustCallHooks_WhenRunningSaveChanges()
        {
            var hooks = new IHook[]
                            {
                                new TimestampPreInsertHook()
                            };

            var context = new LocalContext(hooks);
            var entity = new TimestampedSoftDeletedEntity();
            context.Entities.Add(entity);
            context.SaveChanges();

            Assert.AreEqual(entity.CreatedAt.Date, DateTime.Today);
        }

        [Test]
        public void HookedDbContext_MustNotCallHooks_IfModelIsInvalid()
        {
            var hooks = new IHook[]
                            {
                                new TimestampPreInsertHook()
                            };

            var context = new LocalContext(hooks);
            var validatedEntity = new ValidatedEntity();
            context.ValidatedEntities.Add(validatedEntity);

            try
            { context.SaveChanges(); }
            catch { }

            Assert.AreNotEqual(validatedEntity.CreatedAt.Date, DateTime.Today);
        }

        [Test]
        public void HookedDbContext_MustCallHooks_IfModelIsInvalidButUnchanged()
        {

            var context = new LocalContext();
            context.RegisterHook(new TimestampPreInsertHook());
            var tsEntity = new TimestampedSoftDeletedEntity();
            var valEntity = new ValidatedEntity();

            context.Entities.Add(tsEntity);
            context.Entry(valEntity).State = EntityState.Unchanged;

            Assert.DoesNotThrow(() => context.SaveChanges());

            Assert.AreEqual(tsEntity.CreatedAt.Date, DateTime.Today);
        }

        [Test]
        public void HookedDbContext_AfterConstruction_CanRegisterNewHooks()
        {
            var context = new LocalContext();
            context.RegisterHook(new TimestampPreInsertHook());

            var entity = new TimestampedSoftDeletedEntity();
            context.Entities.Add(entity);
            context.SaveChanges();

            Assert.AreEqual(entity.CreatedAt.Date, DateTime.Today);
        }

        [Test]
        public void HookedDbContext_ShouldNotHook_IfAnyChangedObjectsAreInvalid()
        {
            var context = new LocalContext();
            context.RegisterHook(new TimestampPreInsertHook());
            var tsEntity = new TimestampedSoftDeletedEntity();
            var valEntity = new ValidatedEntity();

            context.Entities.Add(tsEntity);
            context.ValidatedEntities.Add(valEntity);

            Assert.Throws<DbEntityValidationException>(() => context.SaveChanges());

            Assert.AreNotEqual(tsEntity.CreatedAt.Date, DateTime.Today);
        }

        [Test]
        public void HookedDbContext_ShouldHook_IfValidateBeforeSaveIsDisabled_AndChangedObjectsAreInvalid()
        {
            var context = new LocalContext();
            context.Configuration.ValidateOnSaveEnabled = false;
            context.RegisterHook(new TimestampPreInsertHook());
            var tsEntity = new TimestampedSoftDeletedEntity();
            var valEntity = new ValidatedEntity();

            context.Entities.Add(tsEntity);
            context.ValidatedEntities.Add(valEntity);

            Assert.IsTrue(context.GetValidationErrors().Any(x => !x.IsValid));

            Assert.Throws<DbUpdateException>(() => context.SaveChanges());

            Assert.AreEqual(tsEntity.CreatedAt.Date, DateTime.Today);
            Assert.AreEqual(valEntity.CreatedAt.Date, DateTime.Today);
        }

        [Test]
        public void HookedDbContext_ShouldPostHook_IfNoExceptionIsHit()
        {
            var runCheckingHook = new RunCheckedPreInsertHook();
            var hooks = new IHook[]
                            {
                                runCheckingHook,
                                new TimestampPostInsertHook()
                            };


            var context = new LocalContext(hooks);

            var tsEntity = new TimestampedSoftDeletedEntity();
            tsEntity.CreatedAt = DateTime.Now;
            context.Entities.Add(tsEntity);
            context.SaveChanges();

            Assert.IsTrue(runCheckingHook.HasRun);
            Assert.AreEqual(DateTime.Today, tsEntity.ModifiedAt.Value.Date);
        }

        [Test]
        public void HookedDbContext_ShouldNotPostHook_IfExceptionIsHit()
        {
            var runCheckingHook = new RunCheckedPreInsertHook();
            var hooks = new IHook[]
                            {
                                runCheckingHook,
                                new TimestampPostInsertHook()
                            };

            var context = new LocalContext(hooks);

            var valEntity = new ValidatedEntity();
            valEntity.CreatedAt = DateTime.Now;
            context.ValidatedEntities.Add(valEntity);

            Assert.Throws<DbEntityValidationException>(() => context.SaveChanges());

            Assert.IsFalse(runCheckingHook.HasRun);
            Assert.IsFalse(valEntity.ModifiedAt.HasValue);
        }

        [Test]
        public void HookedDbContext_CanLateBindPostActionHooks()
        {
            var context = new LocalContext();
            context.RegisterHook(new TimestampPostInsertHook());

            var tsEntity = new TimestampedSoftDeletedEntity();
            tsEntity.CreatedAt = DateTime.Now;
            context.Entities.Add(tsEntity);
            context.SaveChanges();

            Assert.AreEqual(DateTime.Today, tsEntity.ModifiedAt.Value.Date);
        }

        [Test]
        public void HookedDbContext_MustOnlyHookWhenObjectIsInTheSameState()
        {
            var context = new LocalContext();
            context.RegisterHook(new TimestampPreInsertHook());
            context.RegisterHook(new TimestampPreUpdateHook());

            var tsEntity = new TimestampedSoftDeletedEntity();
            tsEntity.CreatedAt = DateTime.Now;
            context.Entities.Add(tsEntity);
            context.SaveChanges();

            Assert.AreEqual(DateTime.Today, tsEntity.CreatedAt.Date);
            Assert.IsFalse(tsEntity.ModifiedAt.HasValue);
        }

        [Test]
        public void HookedDbContext_NameOrConnectionString()
        {
            var context = new LocalContextWithNameOrConnectionString();
            Assert.That(context.Database.Connection.Database, Is.EqualTo("|DataDirectory|EFHooksDatabase.sdf"));
        }

        [Test]
        public void HookedDbContext_PreActionHookMethod_MustHaveTheContextPassedInTheMetadata()
        {
            var context = new LocalContext();
            var preAction = A.Fake<PreInsertHook<ITimeStamped>>();
            A.CallTo(() => preAction.HookStates).Returns(EntityState.Added);
            
            context.RegisterHook(preAction);

            // We aren't testing the hook here so just set the createdat date so that SaveChanges passes
            var entity = new TimestampedSoftDeletedEntity { CreatedAt = DateTime.Now };
            context.Entities.Add(entity);
            context.SaveChanges();
            A.CallTo(() => preAction.Hook(entity, A<HookEntityMetadata>.That.Matches(m => m.CurrentContext == context))).MustHaveHappened();
        }

        [Test]
        public void HookedDbContext_PostActionHookMethod_MustHaveTheContextPassedInTheMetadata()
        {
            var context = new LocalContext();
            var postAction = A.Fake<PostInsertHook<ITimeStamped>>();
            A.CallTo(() => postAction.HookStates).Returns(EntityState.Added);

            context.RegisterHook(postAction);

            // We aren't testing the hook here
            var entity = new TimestampedSoftDeletedEntity { CreatedAt = DateTime.Now };
            context.Entities.Add(entity);
            context.SaveChanges();
            A.CallTo(() => postAction.Hook(entity, A<HookEntityMetadata>.That.Matches(m => m.CurrentContext == context))).MustHaveHappened();
        }
    }
}