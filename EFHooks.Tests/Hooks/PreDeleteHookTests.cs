using System.Data.Entity;
using NUnit.Framework;

namespace EFHooks.Tests.Hooks
{
    public class PreDeleteHookTests
    {

        private class SoftDeletePreDeleteHook : PreDeleteHook<ISoftDeleted>
        {
			public override bool RequiresValidation
			{
				get { return true; }
			}

            public override void Hook(ISoftDeleted entity, HookEntityMetadata metadata)
            {
                metadata.State = EntityState.Modified;
                entity.IsDeleted = true;
            }
        }

        [Test]
        public void PreDeleteHook_ReassignToModifiedState()
        {
            var hook = new SoftDeletePreDeleteHook();
            var metadata = new HookEntityMetadata(EntityState.Deleted);
            var entity = new TimestampedSoftDeletedEntity();
            hook.Hook(entity, metadata);

            Assert.AreEqual(true, metadata.HasStateChanged);
            Assert.AreEqual(EntityState.Modified, metadata.State);
        } 
    }
}