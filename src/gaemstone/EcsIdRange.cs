using System;

namespace gaemstone
{
	public readonly struct EcsIdRange
		: IEquatable<EcsIdRange>
	{
		public static readonly EcsIdRange Components  = new(0x00, 0xFF);
		public static readonly EcsIdRange ValidTraits = new(0x000000, 0xFFFFFF);


		public readonly uint Min;
		public readonly uint Max;

		public uint Count => Max - Min + 1;

		public EcsIdRange(uint min, uint max)
		{
			if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater or equal to min");
			(Min, Max) = (min, max);
		}

		public bool Contains(EcsId entity)
			=> (entity.ID >= Min) && (entity.ID <= Max);


		public override string ToString()
		{
			var minStr = $"{Min:X}";
			var maxStr = $"{Max:X}";
			var count  = Math.Max(minStr.Length, maxStr.Length);
			minStr = minStr.PadLeft(count, '0');
			maxStr = maxStr.PadLeft(count, '0');
			return $"EcsIdRange(min: 0x{minStr}, max: 0x{maxStr})";
		}


		public bool Equals(EcsIdRange other)
			=> (Min == other.Min) && (Max == other.Max);
		public override bool Equals(object? obj)
			=> (obj is EcsIdRange other) && Equals(other);
		public override int GetHashCode()
			=> HashCode.Combine(Min, Max);

		public static bool operator ==(EcsIdRange left, EcsIdRange right)
			=> left.Equals(right);
		public static bool operator !=(EcsIdRange left, EcsIdRange right)
			=> !left.Equals(right);
	}
}
