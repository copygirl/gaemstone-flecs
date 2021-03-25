using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace gaemstone
{
	public class EntityManager : IReadOnlyDictionary<EntityId, EntityManager.Record>
	{
		public struct Record
		{
			public Archetype Archetype { get; }
			public int Row { get; }
			public EntityType Type => Archetype.Type;
			public Record(Archetype archetype, int row)
				{ Archetype = archetype; Row = row; }
		}

		readonly Universe _universe;
		readonly Dictionary<EntityId, Record> _records = new();

		public int Count => _records.Count;
		public Record this[EntityId id] {
			get => _records[id];
			set => _records[id] = value;
		}

		internal EntityManager(Universe universe) => _universe = universe;

		public bool TryGet(EntityId id, [MaybeNullWhen(false)] out Record value)
			=> _records.TryGetValue(id, out value);


		// IReadOnlyDictionary implementation

		Record IReadOnlyDictionary<EntityId, Record>.this[EntityId key] => _records[key];
		IEnumerable<EntityId> IReadOnlyDictionary<EntityId, Record>.Keys => _records.Keys;
		IEnumerable<Record> IReadOnlyDictionary<EntityId, Record>.Values => _records.Values;

		bool IReadOnlyDictionary<EntityId, Record>.ContainsKey(EntityId key)
			=> _records.ContainsKey(key);
		bool IReadOnlyDictionary<EntityId, Record>.TryGetValue(EntityId key, [MaybeNullWhen(false)] out Record value)
			=> _records.TryGetValue(key, out value);

		public IEnumerator<KeyValuePair<EntityId, Record>> GetEnumerator()
			=> _records.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
