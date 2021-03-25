using System;
using System.Collections.Generic;
using gaemstone.Utility;

namespace gaemstone
{
	public class Archetype
	{
		const int INITIAL_CAPACITY = 64;

		public Universe Universe { get; }
		public EcsType Type { get; }

		Array[]? _columns;
		EcsId[]? _entities;

		public int Count { get; private set; }
		public int Capacity => _entities?.Length ?? 0;

		public Array[] Columns  => _columns  ?? Array.Empty<Array>();
		public EcsId[] Entities => _entities ?? Array.Empty<EcsId>();

		internal Archetype(Universe universe, EcsType type)
		{
			Universe = universe;
			Type     = type;
			InitializeEdges();
		}

		public void Resize(int length)
		{
			if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative");
			if (length < Count) throw new ArgumentOutOfRangeException(nameof(length), "length cannot be smaller than Count");

			// FIXME: Non-components would never have columns. Don't reserve space for them!
			if (_entities == null) {
				_entities = new EcsId[length];
				_columns  = new Array[Type.Count];
				for (var i = 0; i < Type.Count; i++) {
					var arrayType = Universe.GetStruct<Component>(Type[i])?.Type;
					if (arrayType == null) continue;
					_columns[i] = Array.CreateInstance(arrayType, length);
				}
			} else {
				var newEntities = new EcsId[length];
				Array.Copy(_entities, newEntities, Math.Min(_entities.Length, length));
				_entities = newEntities;
				for (var i = 0; i < Type.Count; i++) {
					if (_columns![i] == null) continue;
					var elementType = _columns[i].GetType().GetElementType()!;
					var newColumn   = Array.CreateInstance(elementType, length);
					Array.Copy(_columns[i], newColumn, Math.Min(_columns[i].Length, length));
					_columns[i] = newColumn;
				}
			}
		}

		void EnsureCapacity()
		{
			if (Capacity == 0) Resize(INITIAL_CAPACITY);
			else if (Count >= Capacity) Resize(Capacity * 2);
		}


		internal int Add(EcsId entity)
		{
			EnsureCapacity();
			_entities![Count] = entity;
			return Count++;
		}

		internal void Remove(int row)
		{
			if (row >= Count) throw new ArgumentOutOfRangeException(nameof(row), "row cannot be greater or equal to Count");
			Count--;

			if (row < Count) {
				// Move the last element into the place of the removed one.
				_entities![row] = _entities[Count];
				foreach (var column in _columns!)
					if (column != null)
						Array.Copy(column, Count, column, row, 1);
				// Update the moved element's Record to point to its new row.
				Universe.Entities[_entities[row]] = new(this, row);
			}

			//	Clear out the last element.
			_entities![Count] = default;
			foreach (var column in _columns!)
				if (column != null)
					Array.Clear(column, Count, 1);
		}


		Edge[]? _componentEdges;
		RefDictionary<EcsId, Edge>? _entityEdges;
		struct Edge
		{
			public Archetype? Add;
			public Archetype? Remove;
		}

		void InitializeEdges()
		{
			// Make each edge that is already in Type point to this table.
			foreach (var type in Type) {
				ref var edge = ref GetEdge(type);
				edge.Add = this;
				// If this is the only value in Type, point to
				// the empty RootArchetype when it is removed.
				if (Type.Count == 1) edge.Remove = Universe.RootArchetype;
			}
		}

		public IEnumerable<Archetype> Enumerate()
		{
			yield return this;
			// FIXME: This is not performant, create custom enumeration code!
			if (_componentEdges != null)
				foreach (var edge in _componentEdges)
					if ((edge.Add != null) && (edge.Add != this))
						foreach (var archetype in edge.Add.Enumerate())
							yield return archetype;
			if (_entityEdges != null)
				foreach (var (_, edge) in _entityEdges)
					if ((edge.Add != null) && (edge.Add != this))
						foreach (var archetype in edge.Add.Enumerate())
							yield return archetype;
		}

		ref Edge GetEdge(EcsId id)
		{
			if (EcsIdRange.Components.Contains(id)) {
				if (_componentEdges == null)
					_componentEdges = new Edge[EcsIdRange.Components.Count];
				return ref _componentEdges[id.ID];
			} else {
				if (_entityEdges == null) _entityEdges = new();
				return ref _entityEdges.GetEntry(GetBehavior.Create, id).Value;
			}
		}

		/// <summary>
		/// Connects up the edges of this and the other specified Archetype.
		/// When the specified EntityId is removed from Type, it points to the other.
		/// When the specified EntityId is added to <c>other.Type</c>, it points to this.
		/// </summary>
		internal void ConnectEdges(EcsId removed, Archetype other)
		{
			GetEdge(removed).Remove = other;
			other.GetEdge(removed).Add = this;
		}

		public Archetype With(EcsId id)
		{
			ref var archetype = ref GetEdge(id).Add;
			if (archetype != null) return archetype;
			return archetype = Universe.CreateArchetype(Type.Add(id));
		}
	}
}
