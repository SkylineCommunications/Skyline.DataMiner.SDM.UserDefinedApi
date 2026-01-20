namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;

	public class OrderNode : ODataNode<OrderNode>
	{
		public OrderNode(PropertyNode property, ODataOrder order, OrderNode? next, ODataNode? parent = null) : base(parent)
		{
			Property = property?.WithParent(this) ?? throw new ArgumentNullException(nameof(property));
			Next = next?.WithParent(this);
			Order = order;
		}

		public PropertyNode Property { get; }

		public ODataOrder Order { get; }

		public OrderNode? Next { get; }

		public OrderNode Update(PropertyNode property, ODataOrder order, OrderNode? next, ODataNode? parent)
		{
			if (property != Property || order != Order || next != Next || parent != Parent)
			{
				var newNode = new OrderNode(property, order, next, parent);
				return newNode;
			}

			return this;
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitOrder(this);
		}

		public OrderNode WithProperty(PropertyNode property)
		{
			return Update(property, Order, Next, Parent);
		}

		public OrderNode WithOrder(ODataOrder order)
		{
			return Update(Property, order, Next, Parent);
		}

		public OrderNode WithNext(OrderNode? next)
		{
			return Update(Property, Order, next, Parent);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Property, Order, Next, parent);
		}
	}
}
