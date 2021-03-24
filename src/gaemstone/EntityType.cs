using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace gaemstone
{
	public class EntityType
		: IEquatable<EntityType>, IReadOnlyList<EntityId>
	{
		public static readonly EntityType Empty = new(Enumerable.Empty<EntityId>());


		readonly ImmutableList<EntityId> _entries;
		readonly int _hashCode;

		public int Count => _entries.Count;
		public EntityId this[int index] => _entries[index];

		private EntityType(IEnumerable<EntityId> entries)
		{
			_entries = entries
				.OrderBy(id => id)
				.Distinct()
				.ToImmutableList();

			var hashCode = new HashCode();
			foreach (var id in this) hashCode.Add(id);
			_hashCode = hashCode.ToHashCode();
		}

		public int IndexOf(EntityId value)
			=> _entries.IndexOf(value);
		public bool Contains(EntityId value)
			=> _entries.Contains(value);


		public EntityType Add(params EntityId[] values)
			=> Add((IEnumerable<EntityId>)values);
		public EntityType Add(IEnumerable<EntityId> values)
			=> new(_entries.Concat(values));

		public EntityType Remove(params EntityId[] values)
			=> Remove((IEnumerable<EntityId>)values);
		public EntityType Remove(IEnumerable<EntityId> values)
			=> new(_entries.Except(values));


		public IEnumerator<EntityId> GetEnumerator()
			=> _entries.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public bool Equals(EntityType? other)
			=> (other is not null) && Enumerable.SequenceEqual(_entries, other._entries);
		public override bool Equals(object? obj)
			=> Equals(obj as EntityType);
		public override int GetHashCode() => _hashCode;

		public static bool operator ==(EntityType left, EntityType right)
			=> object.ReferenceEquals(left, right) || left.Equals(right);
		public static bool operator !=(EntityType left, EntityType right)
			=> !(left == right);

		public override string ToString()
			=> $"EntityType({string.Join(", ", this)})";
		public string ToPrettyString(Universe universe)
			=> $"[{string.Join(", ", this.Select(id => id.ToPrettyString(universe)))}]";
	}
}
