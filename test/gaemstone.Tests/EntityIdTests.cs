using Xunit;

namespace gaemstone.Tests
{
	public class EntityIdTests
	{
		[Fact]
		public void Test_Properties()
		{
			var entityWithGen = new EcsId(0x200, 20);
			Assert.Equal((uint)0x200, entityWithGen.ID);
			Assert.Equal((ushort)20, entityWithGen.Generation);
			Assert.Equal(EcsRole.None, entityWithGen.Role);

			// TODO: Due to lack of non-Pair role, create a "fake" role.
			var entityWithRole = new EcsId(0x300, 30, (EcsRole)0xFF);
			Assert.Equal((uint)0x300, entityWithRole.ID);
			Assert.Equal((ushort)30, entityWithRole.Generation);
			Assert.Equal((EcsRole)0xFF, entityWithRole.Role);
		}

		[Fact]
		public void Test_Pairs()
		{
			var relation = new EcsId(0x100);
			var target   = new EcsId(0x200);

			var pair = EcsId.Pair(relation, target);
			Assert.Equal((uint)0x200, pair.ID);
			Assert.Equal(EcsRole.Pair, pair.Role);
			Assert.Equal((relation, target), pair.ToPair());

			var relationWithGen = new EcsId(0x300, 30);
			var targetWithRole  = new EcsId(0x400, 40, (EcsRole)0xFF);

			var pair2 = EcsId.Pair(relationWithGen, targetWithRole);
			Assert.Equal((relation, target), pair.ToPair());
			Assert.Equal((uint)0x300, pair2.ToPair().Relation.ID);
			Assert.Equal((uint)0x400, pair2.ToPair().Target.ID);
			// Pairs will lose any Generation and Role information.
			Assert.NotEqual((relationWithGen, targetWithRole), pair2.ToPair());
			Assert.Equal(0, pair2.ToPair().Relation.Generation);
			Assert.Equal(EcsRole.None, pair2.ToPair().Target.Role);
		}

		[Fact]
		public void Test_ToString()
		{
			var entity         = new EcsId(0x100);
			var entityWithGen  = new EcsId(0x200, 20);
			// TODO: Due to lack of non-Pair role, create "fake" roles.
			var entityWithRole = new EcsId(0x300, (EcsRole)230);
			var entityWithBoth = new EcsId(0x400, 40, (EcsRole)240);
			Assert.Equal("EcsId(id: 0x100)", entity.ToString());
			Assert.Equal("EcsId(id: 0x200, generation: 20)", entityWithGen.ToString());
			Assert.Equal("EcsId(id: 0x300, role: 230)", entityWithRole.ToString());
			Assert.Equal("EcsId(id: 0x400, generation: 40, role: 240)", entityWithBoth.ToString());

			var pair = EcsId.Pair(entity, entityWithGen);
			Assert.Equal("EcsId.Pair(relation: 0x100, target: 0x200)", pair.ToString());
		}
	}
}
