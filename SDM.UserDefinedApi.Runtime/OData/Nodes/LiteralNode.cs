namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public class LiteralNode : ODataNode<LiteralNode>
	{
		internal LiteralNode(ODataPrimitiveType type, object value, ODataNode? parent = null)
			: base(parent)
		{
			Type = type;
			Value = value;
		}

		public ODataPrimitiveType Type { get; }

		public object Value { get; }

		public LiteralNode Update(
			ODataPrimitiveType type,
			object value,
			ODataNode? parent = null)
		{
			if (type != Type || value != Value || parent != Parent)
			{
				var newNode = new LiteralNode(type, value, parent);
				return newNode;
			}

			return this;
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitLiteral(this);
		}

		public LiteralNode WithValue(object value)
		{
			return Update(Type, value, Parent);
		}

		public LiteralNode WithType(ODataPrimitiveType type)
		{
			return Update(type, Value, Parent);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Type, Value, parent);
		}
	}
}
