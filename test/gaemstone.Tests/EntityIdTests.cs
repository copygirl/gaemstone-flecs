using Xunit;

namespace gaemstone.Tests
{
	public class EntityIdTests
	{
		[Fact]
		public void Test_Properties()
		{
			var entityWithGen = new EntityId(0x200, 20);
			Assert.Equal(0x200u, entityWithGen.ID);
			Assert.Equal((ushort)20, entityWithGen.Generation);
			Assert.Equal(EntityRole.None, entityWithGen.Role);
			Assert.Null(entityWithGen.Trait);

			var entityWithRole = new EntityId(0x300, 30, EntityRole.InstanceOf);
			Assert.Equal(0x300u, entityWithRole.ID);
			Assert.Equal((ushort)30, entityWithRole.Generation);
			Assert.Equal(EntityRole.InstanceOf, entityWithRole.Role);
			Assert.Null(entityWithRole.Trait);
		}

		[Fact]
		public void Test_Traits()
		{
			var componentId = new EntityId(0x01);
			var traitId     = new EntityId(0x02);

			var trait = new EntityId(componentId, traitId);
			Assert.Equal(0x01u, trait.ID);
			Assert.Null(trait.Generation);
			Assert.Equal(EntityRole.Trait, trait.Role);
			Assert.Equal((componentId, traitId), trait.Trait);

			var entityWithGen  = new EntityId(0x200, 20);
			var entityWithRole = new EntityId(0x300, 30, EntityRole.InstanceOf);

			var trait2 = new EntityId(entityWithGen, entityWithRole);
			// Traits will lose any Generation and Role information.
			Assert.NotEqual((entityWithGen, entityWithRole), entityWithGen.Trait);
			Assert.Equal(entityWithGen.ID, trait2.Trait!.Value.ComponentId.ID);
			Assert.Equal(entityWithRole.ID, trait2.Trait!.Value.TraitId.ID);
		}

		[Fact]
		public void Test_ToString()
		{
			var entity         = new EntityId(0x100);
			var entityWithGen  = new EntityId(0x200, 20);
			var entityWithRole = new EntityId(0x300, role: EntityRole.InstanceOf);
			var entityWithBoth = new EntityId(0x400, 40, EntityRole.InstanceOf);
			Assert.Equal("EntityId(id: 0x100)", entity.ToString());
			Assert.Equal("EntityId(id: 0x200, generation: 20)", entityWithGen.ToString());
			Assert.Equal("EntityId(id: 0x300, role: InstanceOf)", entityWithRole.ToString());
			Assert.Equal("EntityId(id: 0x400, generation: 40, role: InstanceOf)", entityWithBoth.ToString());

			var trait = new EntityId(entity, entityWithGen);
			Assert.Equal("EntityId(componentId: 0x100, traitId: 0x200)", trait.ToString());
		}
	}
}
