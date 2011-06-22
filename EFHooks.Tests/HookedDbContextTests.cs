using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using EFHooks.Tests.Hooks;
using NUnit.Framework;

namespace EFHooks.Tests
{
    public class HookedDbContextTests
    {
        private class TimestampPreInsertHook : PreInsertHook<ITimeStamped>
        {
            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.CreatedAt = DateTime.Now;
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
    }
}