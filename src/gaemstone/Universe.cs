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
			column[0]  = new(typeof(Component));
			record.Archetype.Columns[0] = column;

			Set(IDENTIFIER_ID, new Component(typeof(Identifier)));
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
		{
			if (GetEntityRecord(entity) is Record record) {
				var oldType = record.Type;
				var newType = oldType.Add(value);
				if (newType == oldType) return record;

				if (!_archetypes.TryGetValue(newType, out var newArchetype))
					newArchetype = CreateArchetype(newType);

				var newRow = newArchetype.Add(entity);

				// Move over entries from old to new archetype.
				var newColumns = ((IEnumerable<Array>)newArchetype.Columns).GetEnumerator();
				foreach (var column in record.Archetype.Columns) {
					newColumns.MoveNext();
					// Skip non-component columns.
					if (column == null) continue;
					// Skip the columns in newArchetype that don't exist in record.Archetype.
					// Since columns are built from EntityType which is sorted, MoveNext works.
					// This could technically be an if-statement, because this can only occur once
					// when adding a single entry, but was left a while-loop for the sake of clarity.
					while (column.GetType() != newColumns.Current?.GetType()) newColumns.MoveNext();
					// Copy value from old column to new one.
					Array.Copy(column, record.Row, newColumns.Current, newRow, 1);
				}

				record.Archetype.Remove(record.Row);
				record.Archetype = newArchetype;
				record.Row       = newRow;
				return record;
			} else {
				var archetype = RootArchetype.With(value);
				record = new(archetype, archetype.Add(entity));
				_entities.Add(entity, record);
				return record;
			}
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
