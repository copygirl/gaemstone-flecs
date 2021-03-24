namespace gaemstone
{
	/// <summary>
	/// Type roles are used to indicate the role of an entity in a type. If no flag
	/// is specified, the entity is interpreted as a regular component or tag.
	/// </summary>
	public enum EntityRole : ulong
	{
		None = 0,

		/// <summary> The TRAIT role indicates that the entity is a trait identifier. </summary>
		Trait = EntityId.ROLE_BIT | 0x7Aul << 56,

		/// <summary>
		/// The INSTANCEOF role indicates that the components from the entity
		/// should be shared with the entity that instantiates the type.
		/// </summary>
		InstanceOf = EntityId.ROLE_BIT | 0x7Eul << 56,

		/// <summary>
		/// The CHILDOF role indicates that the entity should be treated as a
		/// parent of the entity that instantiates the type.
		/// </summary>
		ChildOf = EntityId.ROLE_BIT | 0x7Dul << 56,

/* Currently unsupported Roles:
		/// <summary> Cases are used to switch between mutually exclusive components. </summary>
		Case       = EntityId.ROLE_BIT | 0x7Cul << 56,
		/// <summary> Switches allow for fast switching between mutually exclusive components. </summary>
		Switch     = EntityId.ROLE_BIT | 0x7Bul << 56,
		/// <summary>
		/// Enforce that all entities of a type are present in the type.
		/// This flag can only be used in combination with an entity that has EcsType.
		/// </summary>
		And        = EntityId.ROLE_BIT | 0x79ul << 56,
		/// <summary>
		/// Enforce that at least one entity of a type must be present in the type.
		/// This flag can only be used in combination with an entity that has EcsType.
		/// </summary>
		Or         = EntityId.ROLE_BIT | 0x78ul << 56,
		/// <summary>
		/// Enforce that exactly one entity of a type must be present in the type.
		/// This flag can only be used in combination with an entity that has EcsType.
		/// When another entity of the XOR'd type is added to an entity of this type, the
		/// previous entity is removed from the entity. This makes XOR useful for
		/// implementing state machines, as it allows for traversing states while
		/// ensuring that only one state is ever active at the same time.
		/// </summary>
		Xor        = EntityId.ROLE_BIT | 0x77ul << 56,
		/// <summary>
		/// None of the entities in a type may be added to the type.
		/// This flag can only be used in combination with an entity that has EcsType.
		/// </summary>
		Not        = EntityId.ROLE_BIT | 0x76ul << 56,
		/// <summary> Enforce ownership of a component. </summary>
		Owned      = EntityId.ROLE_BIT | 0x75ul << 56,
		/// <summary> Track whether component is enabled or not. </summary>
		Disabled   = EntityId.ROLE_BIT | 0x74ul << 56,
*/
	}
}
