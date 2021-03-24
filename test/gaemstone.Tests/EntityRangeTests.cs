using Xunit;

namespace gaemstone.Tests
{
	public class EntityRangeTests
	{
		[Fact]
		public void Test_Contains()
		{
			var entity1         = new EntityId(0x20);
			var entity1WithGen  = new EntityId(0x20, 20);
			var entity1WithRole = new EntityId(0x20, role: EntityRole.ChildOf);

			var entity2         = new EntityId(0x200);
			var entity2WithGen  = new EntityId(0x200, 20);
			var entity2WithRole = new EntityId(0x200, role: EntityRole.ChildOf);

			var range1 = EntityRange.Components;
			Assert.True(range1.Contains(entity1));
			Assert.True(range1.Contains(entity1WithGen));
			Assert.True(range1.Contains(entity1WithRole));
			Assert.False(range1.Contains(entity2));
			Assert.False(range1.Contains(entity2WithGen));
			Assert.False(range1.Contains(entity2WithRole));

			var range2 = new EntityRange(0x100, 0xFFFFFFFF);
			Assert.False(range2.Contains(entity1));
			Assert.False(range2.Contains(entity1WithGen));
			Assert.False(range2.Contains(entity1WithRole));
			Assert.True(range2.Contains(entity2));
			Assert.True(range2.Contains(entity2WithGen));
			Assert.True(range2.Contains(entity2WithRole));
		}

		[Fact]
		public void Test_ToString()
		{
			var range1 = EntityRange.Components;
			var range2 = new EntityRange(0x0, 0xF);
			var range3 = new EntityRange(0x100, 0xFFFFFFFF);

			Assert.Equal("EntityRange(min: 0x00, max: 0xFF)"            , range1.ToString());
			Assert.Equal("EntityRange(min: 0x00, max: 0x0F)"            , range2.ToString());
			Assert.Equal("EntityRange(min: 0x00000100, max: 0xFFFFFFFF)", range3.ToString());
		}
	}
}
