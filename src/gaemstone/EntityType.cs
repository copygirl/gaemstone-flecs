using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace gaemstone
{
	public class EntityType
		: IEquatable<EntityType>, IReadOnlyList<EcsId>
	{
		public static readonly EntityType Empty = new(Enumerable.Empty<EcsId>());


		readonly ImmutableList<EcsId> _entries;
		readonly int _hashCode;

		public int Count => _entries.Count;
		public EcsId this[int index] => _entries[index];

		public EntityType(params EcsId[] entries)
			: this((IEnumerable<EcsId>)entries) {  }
		public EntityType(IEnumerable<EcsId> entries)
		{
			_entries = entries
				.OrderBy(id => id)
				.Distinct()
				.ToImmutableList();

			var hashCode = new HashCode();
			foreach (var id in this) hashCode.Add(id);
			_hashCode = hashCode.ToHashCode();
		}

		public int IndexOf(EcsId value)
			=> _entries.IndexOf(value);
		public bool Contains(EcsId value)
			=> _entries.Contains(value);


		public EntityType Add(params EcsId[] values)
			=> Add((IEnumerable<EcsId>)values);
		public EntityType Add(IEnumerable<EcsId> values)
			=> new(_entries.Concat(values));

		public EntityType Remove(params EcsId[] values)
			=> Remove((IEnumerable<EcsId>)values);
		public EntityType Remove(IEnumerable<EcsId> values)
			=> new(_entries.Except(values));


		public IEnumerator<EcsId> GetEnumerator()
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
