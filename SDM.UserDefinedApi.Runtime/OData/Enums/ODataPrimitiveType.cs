namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public enum ODataPrimitiveType
	{
		/// <summary>
		/// Represents the absence of a value
		/// </summary>
		Null,

		/// <summary>
		/// Represent fixed- or variable- length binary data
		/// </summary>
		Binary,

		/// <summary>
		/// Represents the mathematical concept of binary-valued logic
		/// </summary>
		Boolean,

		/// <summary>
		/// Unsigned 8-bit integer value
		/// </summary>
		Byte,

		/// <summary>
		/// Represents a signed 8-bit integer value
		/// </summary>
		SByte,

		/// <summary>
		/// Represents numeric values with fixed precision and scale. This type can describe a numeric value ranging from negative 10^255 + 1 to positive 10^255 -1
		/// </summary>
		Decimal,

		/// <summary>
		/// Represents a floating point number with 15 digits precision that can represent values with approximate range of Â± 2.23e -308 through Â± 1.79e +308
		/// </summary>
		Double,

		/// <summary>
		/// Represents a floating point number with 7 digits precision that can represent values with approximate range of Â± 1.18e -38 through Â± 3.40e +38
		/// </summary>
		Single,

		/// <summary>
		/// Represents a 16-byte (128-bit) unique identifier value
		/// </summary>
		Guid,

		/// <summary>
		/// Represents a signed 16-bit integer value
		/// </summary>
		Int16,

		/// <summary>
		/// Represents a signed 32-bit integer value
		/// </summary>
		Int32,

		/// <summary>
		/// Represents a signed 64-bit integer value
		/// </summary>
		Int64,

		/// <summary>
		/// Represents fixed- or variable-length character data
		/// </summary>
		String,

		/// <summary>
		/// Represents the time of day with values ranging from 0:00:00.x to 23:59:59.y, where x and y depend upon the precision
		/// </summary>
		Time,

		/// <summary>
		/// Represents date and time with values ranging from 12:00:00 midnight, January 1, 1753 A.D. through 11:59:59 P.M, December 9999 A.D.
		/// </summary>
		DateTime,

		/// <summary>
		/// Represents date and time as an Offset in minutes from GMT, with values ranging from 12:00:00 midnight, January 1, 1753 A.D. through 11:59:59 P.M, December 9999 A.D
		/// </summary>
		DateTimeOffset,
	}
}
