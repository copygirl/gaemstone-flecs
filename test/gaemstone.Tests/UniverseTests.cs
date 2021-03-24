using Xunit;

namespace gaemstone.Tests
{
	public class UniverseTests
	{
		[Fact]
		public void Test()
		{
			var universe    = new Universe();
			var componentId = Universe.COMPONENT_ID;
			Assert.Equal(typeof(Component), universe.GetStruct<Component>(componentId)?.Type);
			Assert.Equal(nameof(Component), universe.GetStruct<Identifier>(componentId));

			Assert.Equal(2, universe.GetEntityRecord(componentId)!.Archetype.Count);

			var testId = new EntityId(0x100);
			universe.Add(componentId, testId);
			Assert.Equal(new []{ componentId, Universe.IDENTIFIER_ID, testId },
			             universe.GetEntityRecord(componentId)!.Type);

			Assert.Equal(1, universe.GetEntityRecord(componentId)!.Archetype.Count);
		}

		[Fact]
		public void Test_ToPrettyString()
		{
			var universe = new Universe();
			Assert.Equal("Component", Universe.COMPONENT_ID.ToPrettyString(universe));
			Assert.Equal("Identifier", Universe.IDENTIFIER_ID.ToPrettyString(universe));

			var testId = new EntityId(0x100);
			universe.Add(Universe.COMPONENT_ID, testId);
			Assert.Equal("[Component, Identifier, 0x100]",
			             universe.GetEntityType(Universe.COMPONENT_ID)!.ToPrettyString(universe));
		}
	}
}
