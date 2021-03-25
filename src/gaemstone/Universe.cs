using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace gaemstone
{
	public class Universe
	{
		public static readonly EntityId COMPONENT_ID  = new(0x01);
		public static readonly EntityId IDENTIFIER_ID = new(0x02);


		readonly Dictionary<EntityId, Record> _entities = new();
		public IReadOnlyDictionary<EntityId, Record> Entities { get; }

		public class Record
		{
			public Archetype Archetype { get; set; }
			public int Row { get; set; }
			public EntityType Type => Archetype.Type;
			public Record(Archetype archetype, int row)
				{ Archetype = archetype; Row = row; }
		}

		readonly Dictionary<EntityType, Archetype> _archetypes = new();
		public Archetype RootArchetype { get; }


		public Universe()
		{
			Entities = new ReadOnlyDictionary<EntityId, Record>(_entities);
			RootArchetype = CreateArchetype(EntityType.Empty);

			// Bootstrap the initial `[Component]` Archetype so other methods work in the first place.
			var record = Add(COMPONENT_ID, COMPONENT_ID);
			var column = new Component[record.Archetype.Capacity];
			column[0]  = Component.Of<Component>();
			record.Archetype.Columns[0] = column;

			Set(IDENTIFIER_ID, Component.Of<Identifier>());
			Set(COMPONENT_ID, new Identifier(nameof(Component)));
			Set(IDENTIFIER_ID, new Identifier(nameof(Identifier)));
		}


		public Archetype CreateArchetype(EntityType type)
		{
			var archetype = new Archetype(this, type);
			_archetypes.Add(type, archetype);
			foreach (var id in type)
				if (_archetypes.TryGetValue(type.Remove(id), out var otherArchetype))
					archetype.ConnectEdges(id, otherArchetype);
			return archetype;
		}


		public Record Add(EntityId entity, EntityId value)
			=> ModifyEntityType(entity, type => type.Add(value));
		public Record Remove(EntityId entity, EntityId value)
			=> ModifyEntityType(entity, type => type.Remove(value));

		public Record SetEntityType(EntityId entity, EntityType type)
			=> ModifyEntityType(entity, _ => type);
		public Record ModifyEntityType(EntityId entity, Func<EntityType, EntityType> func)
		{
			var record = GetEntityRecord(entity);
			var type   = func(record?.Type ?? EntityType.Empty);
			if (!_archetypes.TryGetValue(type, out var target))
				target = CreateArchetype(type);

			if (record == null) {
				record = new(target, target.Add(entity));
				_entities.Add(entity, record);
			} else if (target != record.Archetype)
				MoveRow(entity, record, target);

			return record;
		}

		static void MoveRow(EntityId entity, Record from, Archetype to)
		{
			// Add the entity to the new Archetype, and get the row index.
			var newRow = to.Add(entity);

			var oldType = from.Archetype.Type;
			var newType = to.Type;

			// Iterate the old and new types and when they overlap (have the
			// same entry), attempt to move data over to the new Archetype.
			var oldIndex = 0;
			var newIndex = 0;
			while ((oldIndex < oldType.Count) && (newIndex < newType.Count)) {
				var diff = oldType[oldIndex].CompareTo(newType[newIndex]);
				if (diff == 0) {
					// Only copy if the column actually exists (is a component).
					if (from.Archetype.Columns[oldIndex] is Array column)
						Array.Copy(column, from.Row, to.Columns[newIndex], newRow, 1);
					newIndex++;
					oldIndex++;
				}
				// If the entries are not the same, advance only one of them.
				// Since the entries in EntityType are sorted, we can do this.
				else   if (diff > 0)   newIndex++;
				else /*if (diff < 0)*/ oldIndex++;
			}

			from.Archetype.Remove(from.Row);
			from.Archetype = to;
			from.Row       = newRow;
		}


		public void Set<T>(EntityId entity, T value)
		{
			var componentId = GetComponentTypeIdOrThrow(typeof(T));
			var record      = Add(entity, componentId);
			var columnIndex = record.Type.IndexOf(componentId);
			((T[])record.Archetype.Columns[columnIndex])[record.Row] = value;
		}


		public EntityId? GetComponentTypeId(Type type)
		{
			foreach (var archetype in RootArchetype.With(COMPONENT_ID).Enumerate()) {
				var componentColumn = archetype.Columns.OfType<Component[]>().FirstOrDefault();
				if (componentColumn == null) continue;
				var index = Array.FindIndex(componentColumn, component => component.Type == type);
				if (index < 0) continue;
				return archetype.Entities[index];
			}
			return null;
		}
		public EntityId GetComponentTypeIdOrThrow(Type type)
			=> GetComponentTypeId(type) ?? throw new InvalidOperationException(
				$"The specified component type {type.Name} cannot be found as an entity");


		public Record? GetEntityRecord(EntityId entity)
			=> _entities.TryGetValue(entity, out var value) ? value : null;

		public EntityType? GetEntityType(EntityId entity)
			=> GetEntityRecord(entity)?.Archetype?.Type;


		public T? Get<T>(EntityId entity) where T : class
			=> (GetEntityRecord(entity) is Record record)
				? record.Archetype.Columns.OfType<T[]>().First()[record.Row]
				: null;

		public T? GetStruct<T>(EntityId entity) where T : struct
			=> (GetEntityRecord(entity) is Record record)
				? record.Archetype.Columns.OfType<T[]>().First()[record.Row]
				: null;
	}
}
