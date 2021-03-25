using System;
using System.Collections.Generic;
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

			Assert.Equal(2, universe.Entities[componentId].Archetype.Count);

			var testId = new EcsId(0x100);
			universe.Add(componentId, testId);
			Assert.Equal(new []{ componentId, Universe.IDENTIFIER_ID, testId },
			             universe.Entities[componentId].Type);

			Assert.Equal(1, universe.Entities[componentId].Archetype.Count);
		}

		[Fact]
		public void Test_ModifyEntityType()
		{
			var universe = new Universe();
			var entity   = new EcsId(0x100);

			// var testComponent = new EntityId(0x10);
			// universe.Set(testComponent, Component.Of<TestComponent>());

			var value1 = new EcsId(0x101);
			var value2 = new EcsId(0x102);
			var value3 = new EcsId(0x103);

			universe.ModifyEntityType(entity, previousType => {
				Assert.Equal(EcsType.Empty, previousType);
				return new(value1, value2, value3);
			});

			universe.ModifyEntityType(entity, previousType => {
				Assert.Equal(new(value1, value2, value3), previousType);
				return new();
			});

			Assert.Equal(new(), universe.GetEntityType(entity));
		}

		[Fact]
		public void Test_Add()
		{
			var universe = new Universe();

			var entity1 = new EcsId(0x100);
			var entity2 = new EcsId(0x200);
			var entity3 = new EcsId(0x300);
			var entity4 = new EcsId(0x400);

			Assert.False(universe.Entities.TryGet(entity1, out _));
			Assert.Throws<KeyNotFoundException>(() => universe.Entities[entity1]);
			// Same should be true for the other entities...

			universe.Add(entity1, entity2);
			universe.Add(entity1, entity3);
			universe.Add(entity1, entity4);

			universe.Add(entity2, entity3);
			universe.Add(entity2, entity4);

			universe.Add(entity3, entity4);

			Assert.Equal(new []{ entity2, entity3, entity4 }, universe.GetEntityType(entity1));
			Assert.Equal(new []{ entity3, entity4          }, universe.GetEntityType(entity2));
			Assert.Equal(new []{ entity4                   }, universe.GetEntityType(entity3));
		}

		[Fact]
		public void Test_Set()
		{
			var universe = new Universe();
			var entity   = new EcsId(0x100);

			// Adding a component that's not known as a Component in Universe causes an exception.
			Assert.Throws<InvalidOperationException>(() => universe.Set(entity, default(TestComponent)));

			var testComponent = new EcsId(0x10);
			universe.Set(testComponent, Component.Of<TestComponent>());

			universe.Set(entity, new TestComponent(10, 20));
			Assert.Equal(10, universe.GetStruct<TestComponent>(entity)!.Value.X);
			Assert.Equal(20, universe.GetStruct<TestComponent>(entity)!.Value.Y);
		}

		[Fact]
		public void Test_Set_MoveArchetype()
		{
			var universe = new Universe();
			var entity   = new EcsId(0x100);

			var testComponent = new EcsId(0x10);
			universe.Set(testComponent, Component.Of<TestComponent>());

			universe.Set(entity, new TestComponent(10, 20));
			Assert.Equal(10, universe.GetStruct<TestComponent>(entity)!.Value.X);
			Assert.Equal(20, universe.GetStruct<TestComponent>(entity)!.Value.Y);

			var flag = new EcsId(0x200);
			universe.Add(entity, flag);
			// After changing entity's Type by adding flag, component values should've been moved to another Archetype.
			Assert.Equal(10, universe.GetStruct<TestComponent>(entity)!.Value.X);
			Assert.Equal(20, universe.GetStruct<TestComponent>(entity)!.Value.Y);
		}

		[Fact]
		public void Test_ToPrettyString()
		{
			var universe = new Universe();
			Assert.Equal("Component", Universe.COMPONENT_ID.ToPrettyString(universe));
			Assert.Equal("Identifier", Universe.IDENTIFIER_ID.ToPrettyString(universe));

			var testId = new EcsId(0x100);
			universe.Add(Universe.COMPONENT_ID, testId);
			Assert.Equal("[Component, Identifier, 0x100]",
			             universe.GetEntityType(Universe.COMPONENT_ID).ToPrettyString(universe));
		}

		struct TestComponent
		{
			public int X; public byte Y;
			public TestComponent(int x, byte y) { X = x; Y = y; }
		}
	}
}
