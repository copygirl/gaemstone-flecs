using System;
using System.Runtime.InteropServices;
using System.Text;

namespace gaemstone
{
	// Reserved for Components
	//  |     Regular Entities
	//  |       |   Generation        Role
	//  v       v         v            v
	// |8-|-----24------|-16-|#unused#|8-|
	// ===================================
	// |  Low 32 bits   |  High 32 bits  |
	// ===================================

	// When Role == Pair, this is the layout:
	// |-------32-------|-----24------|##|
	//       Target        Relation

	[StructLayout(LayoutKind.Explicit)]
	public readonly struct EcsId
		: IEquatable<EcsId>
		, IComparable<EcsId>
	{
		[FieldOffset(0)]
		public readonly ulong Value;

		[FieldOffset(0)]
		public readonly uint Low;
		[FieldOffset(4)]
		public readonly uint High;

		[FieldOffset(0)]
		public readonly uint ID;
		[FieldOffset(4)]
		public readonly ushort Generation;
		[FieldOffset(7)]
		public readonly EcsRole Role;

		EcsId(ulong value) : this() => Value = value;
		EcsId(uint low, uint high) : this() { Low = low; High = high; }

		public EcsId(uint id) : this() { ID = id; }
		public EcsId(uint id, ushort generation) : this() { ID = id; Generation = generation; }
		public EcsId(uint id, EcsRole role) : this() { ID = id; Role = role; }
		public EcsId(uint id, ushort generation, EcsRole role) : this() { ID = id; Generation = generation; Role = role; }

		public static EcsId Pair(EcsId relation, EcsId target)
			=> new(target.ID | (relation.Value & 0xFFFFFF) << 32 | (ulong)EcsRole.Pair << 56);
		public (EcsId Relation, EcsId Target) ToPair()
			=> (new(High & 0xFFFFFF), new(Low));


		public bool Equals(EcsId other)
			=> Value == other.Value;
		public override bool Equals(object? obj)
			=> (obj is EcsId other) && Equals(other);
		public override int GetHashCode()
			=> Value.GetHashCode();

		public static bool operator ==(EcsId left, EcsId right)
			=> left.Equals(right);
		public static bool operator !=(EcsId left, EcsId right)
			=> !left.Equals(right);

		public int CompareTo(EcsId other)
			=> Value.CompareTo(other.Value);


		public override string ToString()
		{
			if (Role == EcsRole.Pair) {
				return $"EcsId.Pair(relation: 0x{High & 0xFFFFFF:X}, target: 0x{Low:X})";
			} else {
				var sb = new StringBuilder("EcsId(id: 0x").AppendFormat("{0:X}", ID);
				if (Generation != 0) sb.Append(", generation: ").Append(Generation);
				if (Role != EcsRole.None) sb.Append(", role: ").Append(Role);
				return sb.Append(')').ToString();
			}
		}

		// TODO: Move this out of this type.
		public string ToPrettyString(Universe universe)
		{
			if (Role == EcsRole.Pair) {
				var (relation, target) = ToPair();
				string relationStr = universe.GetStruct<Identifier>(relation) ?? $"0x{relation.ID:X}";
				string targetStr   = universe.GetStruct<Identifier>(target)   ?? $"0x{target.ID:X}";
				return $"{relationStr} + {targetStr})";
			} else {
				var id = universe.GetStruct<Identifier>(this);
				var sb = new StringBuilder();
				if (Role != EcsRole.None) sb.Append(Role).Append(" | ");
				return sb.Append(id ?? $"0x{ID:X}").ToString();
			}
		}
	}

	public enum EcsRole : byte
	{
		None,
		Pair,
	}
}
