using System;
using System.Text;

namespace gaemstone
{
	// Reserved for Components
	//   |    Regular Entities  Type Roles
	//   |       |  Generation         |
	//   v       v        v            v
	// |-8-|-----24-----|-16-|       |-8-|
	// ===================================
	// |  Low 32 bits   |  High 32 bits  |
	// ===================================
	//                  |-----24-----|
	//                     Trait ID

	public readonly struct EntityId
		: IEquatable<EntityId>, IComparable<EntityId>
	{
		/// <summary> Role bit added to <see cref="EntityRole"/>s to differentiate between roles and generations. </summary>
		/// <devremarks> Though it seems like they don't overlap in flecs' current setup. </devremarks>
		internal const ulong ROLE_BIT = 1ul << 63;

		const ulong ROLE_MASK       = 0xFF00_0000_0000_0000;
		const ulong ENTITY_MASK     = 0x0000_0000_FFFF_FFFF;
		const ulong GENERATION_MASK = 0x0000_FFFF_0000_0000;
		const uint  TRAIT_MASK      = 0x00FF_FFFF;

		public static readonly EntityId None = default;


		public readonly ulong Value;

		internal EntityId(ulong value) => Value = value;
		public static EntityId FromULong(ulong value) => new(value);

		public EntityId(uint id, uint generation = 0, EntityRole role = EntityRole.None)
		{
			if (generation > 0xFFFF) throw new ArgumentOutOfRangeException(nameof(generation),
				$"generation ({generation}) is larger than the maximum value (65535)");
			Value = id | ((ulong)generation << 32) | (ulong)role;
		}
		public EntityId(EntityId componentId, EntityId traitId)
		{
			if ((traitId.ID & ~TRAIT_MASK) != 0) throw new ArgumentOutOfRangeException(nameof(traitId),
				$"traitId.ID (0x{traitId.ID:X8}) is larger than the maximum value (0xFFFFFF)");
			Value = (componentId.Value & ENTITY_MASK) | (traitId.Value << 32) | (ulong)EntityRole.Trait;
		}


		public uint ID => (uint)Value;
		internal uint GenerationUnsafe => (uint)((Value & GENERATION_MASK) >> 32);
		public uint? Generation => !HasRole(EntityRole.Trait) ? GenerationUnsafe : null;

		internal bool HasRoleBitSet => (Value & ROLE_BIT) != 0;
		internal EntityRole RoleUnsafe => (EntityRole)(Value & ROLE_MASK);
		public EntityRole Role => HasRoleBitSet ? RoleUnsafe : EntityRole.None;
		public bool HasRole(EntityRole role) => (Value & ROLE_MASK) == (ulong)role;

		internal EntityId ComponentIdUnsafe => new(Value & ENTITY_MASK);
		internal EntityId TraitIdUnsafe     => new((Value >> 32) & TRAIT_MASK);
		public (EntityId ComponentId, EntityId TraitId)? Trait
			=> HasRole(EntityRole.Trait) ? (ComponentIdUnsafe, TraitIdUnsafe) : null;


		public bool Equals(EntityId other)
			=> Value == other.Value;
		public override bool Equals(object? obj)
			=> (obj is EntityId other) && Equals(other);
		public override int GetHashCode()
			=> Value.GetHashCode();

		public int CompareTo(EntityId other)
			=> Value.CompareTo(other.Value);

		public static bool operator ==(EntityId left, EntityId right)
			=> left.Equals(right);
		public static bool operator !=(EntityId left, EntityId right)
			=> !left.Equals(right);


		public override string ToString()
		{
			if (Role == EntityRole.Trait) {
				return $"EntityId(componentId: 0x{ComponentIdUnsafe.ID:X}, traitId: 0x{TraitIdUnsafe.ID:X})";
			} else {
				var sb = new StringBuilder("EntityId(id: 0x").AppendFormat("{0:X}", ID);
				if (GenerationUnsafe != 0) sb.Append(", generation: ").Append(GenerationUnsafe);
				if (HasRoleBitSet) sb.Append(", role: ").Append(RoleUnsafe);
				return sb.Append(')').ToString();
			}
		}

		public string ToPrettyString(Universe universe)
		{
			if (Role == EntityRole.Trait) {
				var componentId = universe.GetStruct<Identifier>(ComponentIdUnsafe);
				var traitId     = universe.GetStruct<Identifier>(TraitIdUnsafe);
				return $"Trait | {componentId ?? $"0x{ComponentIdUnsafe.ID:X}"} + {traitId ?? $"0x{TraitIdUnsafe.ID:X})"})";
			} else {
				var id = universe.GetStruct<Identifier>(this);
				var sb = new StringBuilder();
				if (HasRoleBitSet) sb.Append(RoleUnsafe).Append(" | ");
				return sb.Append(id ?? $"0x{ID:X}").ToString();
			}
		}


		// public static EntityId operator |(EntityRole left, EntityId right)
		// {
		// 	if (right.HasRoleBitSet) throw new ArgumentException("EntityId operand cannot have a role set");
		// 	return new(right.Value | (uint)left);
		// }
	}
}
