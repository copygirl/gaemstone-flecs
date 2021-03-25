using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace gaemstone
{
	public class EntityManager : IReadOnlyDictionary<EcsId, EntityManager.Record>
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
		readonly Dictionary<EcsId, Record> _records = new();

		public int Count => _records.Count;
		public Record this[EcsId id] {
			get => _records[id];
			set => _records[id] = value;
		}

		internal EntityManager(Universe universe) => _universe = universe;

		public bool TryGet(EcsId id, [MaybeNullWhen(false)] out Record value)
			=> _records.TryGetValue(id, out value);


		// public EntityId New()
		// {

		// }

		// public void Delete(EntityId id)
		// {
		// 	var record = this[id];
		// 	record.Archetype.Remove(record.Row);
		// 	_records.Remove(id);
		// }


		// IReadOnlyDictionary implementation

		Record IReadOnlyDictionary<EcsId, Record>.this[EcsId key] => _records[key];
		IEnumerable<EcsId> IReadOnlyDictionary<EcsId, Record>.Keys => _records.Keys;
		IEnumerable<Record> IReadOnlyDictionary<EcsId, Record>.Values => _records.Values;

		bool IReadOnlyDictionary<EcsId, Record>.ContainsKey(EcsId key)
			=> _records.ContainsKey(key);
		bool IReadOnlyDictionary<EcsId, Record>.TryGetValue(EcsId key, [MaybeNullWhen(false)] out Record value)
			=> _records.TryGetValue(key, out value);

		public IEnumerator<KeyValuePair<EcsId, Record>> GetEnumerator()
			=> _records.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
