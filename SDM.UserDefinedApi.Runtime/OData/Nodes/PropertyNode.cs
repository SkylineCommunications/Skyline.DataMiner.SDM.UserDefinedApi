namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;

	public class PropertyNode : ODataNode<PropertyNode>
	{
		public PropertyNode(string path, ODataNode? parent = null)
			: base(parent)
		{
			Path = path ?? throw new ArgumentNullException(nameof(path));
		}

		public string Path { get; }

		public PropertyNode Update(
			string path,
			ODataNode? parent)
		{
			if (path != Path || parent != Parent)
			{
				var newNode = new PropertyNode(path, parent);
				return newNode;
			}

			return this;
		}

		public PropertyNode WithPath(string path)
		{
			return Update(path, Parent);
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitProperty(this);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Path, parent);
		}
	}
}
