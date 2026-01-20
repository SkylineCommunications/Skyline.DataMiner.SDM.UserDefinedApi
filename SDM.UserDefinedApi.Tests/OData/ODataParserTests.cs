namespace Skyline.DataMiner.SDM.UserDefinedApi.Tests.OData
{
	using FluentAssertions;

	using Skyline.DataMiner.SDM.UserDefinedApi.OData;

	[TestClass]
	public class ODataParserTests
	{
		[TestMethod]
		public void Empty_Filter()
		{
			// Arrange
			var filter = "";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();
			tree.Filter.Should().BeNull();
			tree.Parent.Should().BeNull();
		}

		[TestMethod]
		public void String_Equal()
		{
			// Arrange
			var filter = "Name eq 'Andi Tamer'";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<BinaryNode>();
			filterNode.Parent.Should().BeNull();

			var binaryNode = filterNode.Filter.As<BinaryNode>();
			binaryNode.Operation.Should().Be(ODataLogicalOperation.Equal);
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("Name");
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Value.Should().Be("Andi Tamer");
			binaryNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void String_Equal_Inverted()
		{
			// Arrange
			var filter = "not Name eq 'Andi Tamer'";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<NotNode>();
			filterNode.Parent.Should().BeNull();

			var notNode = filterNode.Filter.As<NotNode>();
			notNode.Child.Should().BeOfType<BinaryNode>();
			notNode.Parent.Should().Be(filterNode);

			var binaryNode = notNode.Child.As<BinaryNode>();
			binaryNode.Operation.Should().Be(ODataLogicalOperation.Equal);
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("Name");
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Value.Should().Be("Andi Tamer");
			binaryNode.Parent.Should().Be(notNode);
		}

		[TestMethod]
		public void String_StartsWith()
		{
			// Arrange
			var filter = "startswith(Name, 'Andi')";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.As<FunctionCallNode>();
			filterNode.Parent.Should().BeNull();

			var functionCallNode = filterNode.Filter.As<FunctionCallNode>();
			functionCallNode.Name.Should().Be("startswith");
			functionCallNode.Arguments[0].Should().BeOfType<PropertyNode>();
			functionCallNode.Arguments[0].As<PropertyNode>().Path.Should().Be("Name");
			functionCallNode.Arguments[1].Should().BeOfType<LiteralNode>();
			functionCallNode.Arguments[1].As<LiteralNode>().Value.Should().Be("Andi");
			functionCallNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void String_Contains()
		{
			// Arrange
			var filter = "contains(Name, 'Andi')";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<FunctionCallNode>();
			filterNode.Parent.Should().BeNull();

			var functionCallNode = filterNode.Filter.As<FunctionCallNode>();
			functionCallNode.Name.Should().Be("contains");
			functionCallNode.Arguments[0].Should().BeOfType<PropertyNode>();
			functionCallNode.Arguments[0].As<PropertyNode>().Path.Should().Be("Name");
			functionCallNode.Arguments[1].Should().BeOfType<LiteralNode>();
			functionCallNode.Arguments[1].As<LiteralNode>().Value.Should().Be("Andi");
			functionCallNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void Number_NotEqual()
		{
			// Arrange
			var filter = "Age ne 25";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<BinaryNode>();
			filterNode.Parent.Should().BeNull();

			var binaryNode = filterNode.Filter.As<BinaryNode>();
			binaryNode.Operation.Should().Be(ODataLogicalOperation.NotEqual);
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("Age");
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Value.Should().Be(25);
			binaryNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void Number_GreaterThan()
		{
			// Arrange
			var filter = "Age gt 25";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<BinaryNode>();
			filterNode.Parent.Should().BeNull();

			var binaryNode = filterNode.Filter.As<BinaryNode>();
			binaryNode.Operation.Should().Be(ODataLogicalOperation.GreaterThan);
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("Age");
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Value.Should().Be(25);
			binaryNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void Number_GreaterThanOrEqual()
		{
			// Arrange
			var filter = "Age ge 25";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<BinaryNode>();
			filterNode.Parent.Should().BeNull();

			var binaryNode = filterNode.Filter.As<BinaryNode>();
			binaryNode.Operation.Should().Be(ODataLogicalOperation.GreaterThanOrEqual);
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("Age");
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Value.Should().Be(25);
			binaryNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void Number_LessThan()
		{
			// Arrange
			var filter = "Age lt 25";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<BinaryNode>();
			filterNode.Parent.Should().BeNull();

			var binaryNode = filterNode.Filter.As<BinaryNode>();
			binaryNode.Operation.Should().Be(ODataLogicalOperation.LessThan);
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("Age");
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Value.Should().Be(25);
			binaryNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void Number_LessThanOrEqual()
		{
			// Arrange
			var filter = "Age le 25";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<BinaryNode>();
			filterNode.Parent.Should().BeNull();

			var binaryNode = filterNode.Filter.As<BinaryNode>();
			binaryNode.Operation.Should().Be(ODataLogicalOperation.LessThanOrEqual);
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("Age");
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Value.Should().Be(25);
			binaryNode.Parent.Should().Be(filterNode);
		}

		[TestMethod]
		public void And()
		{
			// Arrange
			var filter = "'Andi Tamer' eq Name and Age ge 25";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<AndNode>();

			var andNode = filterNode.Filter.As<AndNode>();
			andNode.Nodes.Should().HaveCount(2);

			foreach (var node in andNode.Nodes)
			{
				node.Parent.Should().Be(andNode);
			}
		}

		[TestMethod]
		public void Group_And()
		{
			// Arrange
			var filter = "Age ge 25 and (Heigth gt 160 and Heigth le 180)";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<AndNode>();
			filterNode.Parent.Should().BeNull();

			var andNode = filterNode.Filter.As<AndNode>();
			tree.Filter.As<AndNode>().Nodes.Should().HaveCount(2);

			var firstNode = andNode.Nodes.ElementAt(0);
			firstNode.Should().BeOfType<BinaryNode>();
			firstNode.Parent.Should().Be(andNode);

			var secondNode = andNode.Nodes.ElementAt(1);
			secondNode.Should().BeOfType<AndNode>();
			secondNode.Parent.Should().Be(andNode);
			secondNode.As<AndNode>().Nodes.Should().HaveCount(2);

			var secondNodeFirstChild = secondNode.As<AndNode>().Nodes.ElementAt(0);
			secondNodeFirstChild.Should().BeOfType<BinaryNode>();
			secondNodeFirstChild.Parent.Should().Be(secondNode);
			var secondNodeSecondChild = secondNode.As<AndNode>().Nodes.ElementAt(1);
			secondNodeSecondChild.Should().BeOfType<BinaryNode>();
			secondNodeSecondChild.Parent.Should().Be(secondNode);
		}

		[TestMethod]
		public void Or()
		{
			// Arrange
			var filter = "'Andi Tamer' eq Name or Age ge 25";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();

			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<OrNode>();
			filterNode.Parent.Should().BeNull();

			var orNode = filterNode.Filter.As<OrNode>();
			orNode.Nodes.Should().HaveCount(2);

			var firstNode = orNode.Nodes.ElementAt(0);
			firstNode.Should().BeOfType<BinaryNode>();
			firstNode.Parent.Should().Be(orNode);
			var secondNode = orNode.Nodes.ElementAt(1);
			secondNode.Should().BeOfType<BinaryNode>();
			secondNode.Parent.Should().Be(orNode);
		}

		[TestMethod]
		public void Collection_Any()
		{
			// Arrange
			var filter = "ExternalIdentifiers/any(e: e/ExternalName eq 'Andi Tamer')";
			var parser = new ODataFilterParser(filter);

			// Act
			var tree = parser.Parse();

			// Assert
			tree.Should().BeOfType<FilterNode>();
			var filterNode = tree.As<FilterNode>();
			filterNode.Filter.Should().BeOfType<MethodCallNode>();

			var methodCallNode = tree.Filter.As<MethodCallNode>();
			methodCallNode.Target.Should().BeOfType<PropertyNode>();
			methodCallNode.Variable.Should().BeOfType<PropertyNode>();
			methodCallNode.Body.Should().BeOfType<BinaryNode>();
			methodCallNode.Parent.Should().Be(filterNode);

			var binaryNode = methodCallNode.Body.As<BinaryNode>();
			binaryNode.Left.Should().BeOfType<PropertyNode>();
			binaryNode.Left.As<PropertyNode>().Path.Should().Be("e/ExternalName");
			binaryNode.Operation.Should().Be(ODataLogicalOperation.Equal);
			binaryNode.Right.Should().BeOfType<LiteralNode>();
			binaryNode.Right.As<LiteralNode>().Type.Should().Be(ODataPrimitiveType.String);
			binaryNode.Right.As<LiteralNode>().Value.Should().Be("Andi Tamer");
			binaryNode.Parent.Should().Be(methodCallNode);
		}
	}
}