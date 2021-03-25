using Xunit;

namespace gaemstone.Tests
{
	public class EntityRangeTests
	{
		[Fact]
		public void Test_Contains()
		{
			var entity1         = new EcsId(0x20);
			var entity1WithGen  = new EcsId(0x20, 20);
			var entity1WithRole = new EcsId(0x20, (EcsRole)0xFF);

			var entity2         = new EcsId(0x200);
			var entity2WithGen  = new EcsId(0x200, 20);
			var entity2WithRole = new EcsId(0x200, (EcsRole)0xFF);

			var range1 = EcsIdRange.Components;
			Assert.True(range1.Contains(entity1));
			Assert.True(range1.Contains(entity1WithGen));
			Assert.True(range1.Contains(entity1WithRole));
			Assert.False(range1.Contains(entity2));
			Assert.False(range1.Contains(entity2WithGen));
			Assert.False(range1.Contains(entity2WithRole));

			var range2 = new EcsIdRange(0x100, 0xFFFFFFFF);
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
			var range1 = EcsIdRange.Components;
			var range2 = new EcsIdRange(0x0, 0xF);
			var range3 = new EcsIdRange(0x100, 0xFFFFFFFF);

			Assert.Equal("EcsIdRange(min: 0x00, max: 0xFF)"            , range1.ToString());
			Assert.Equal("EcsIdRange(min: 0x0, max: 0xF)"              , range2.ToString());
			Assert.Equal("EcsIdRange(min: 0x00000100, max: 0xFFFFFFFF)", range3.ToString());
		}
	}
}
