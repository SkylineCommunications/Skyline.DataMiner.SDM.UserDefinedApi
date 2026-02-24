namespace Skyline.DataMiner.SDM.UserDefinedApi.Tests.OData
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;

	using FluentAssertions;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.Exposers;
	using Skyline.DataMiner.SDM.UserDefinedApi.OData;
	using Skyline.DataMiner.SDM.UserDefinedApi.Tests.Equality;

	using SLDataGateway.API.Querying;

	public class Person : SdmObject<Person>
	{
		public Person(string id, string name, int age)
		{
			Identifier = id;
			Name = name;
			Age = age;
		}

		public override string Identifier { get; set; }

		public string Name { get; set; }

		public int Age { get; }

		public ICollection<string> Aliases { get; } = new List<string>();

		public ICollection<ExternalIdentifier> ExternalIdentifiers { get; } = new List<ExternalIdentifier>();
	}

	public class ExternalIdentifier
	{
		public string ExternalID { get; set; }

		public string ExternalName { get; set; }
	}

	public static class PersonExposers
	{
		static PersonExposers()
		{
			RuntimeHelpers.RunClassConstructor(typeof(ExternalIdentifiers).TypeHandle);
		}

		public static readonly Exposer<Person, string> Identifier = new Exposer<Person, string>((obj) => obj.Identifier, "Identifier");
		public static readonly Exposer<Person, string> Name = new Exposer<Person, string>((obj) => obj.Name, "Name");
		public static readonly Exposer<Person, int> Age = new Exposer<Person, int>((obj) => obj.Age, "Age");
		public static readonly CollectionExposer<Person, string> Aliases = new CollectionExposer<Person, string>((obj) => obj.Aliases.Where(x => x is not null), "Aliases");

		public static class ExternalIdentifiers
		{
			public static readonly CollectionExposer<Person, string> ExternalID = new CollectionExposer<Person, string>((obj) => obj.ExternalIdentifiers.Where(x => x.ExternalID is not null).Select(x => x.ExternalID), "ExternalIdentifiers.ExternalID");
			public static readonly CollectionExposer<Person, string> ExternalName = new CollectionExposer<Person, string>((obj) => obj.ExternalIdentifiers.Where(x => x.ExternalID is not null).Select(x => x.ExternalName), "ExternalIdentifiers.ExternalName");
		}
	}

	[TestClass]
	public class ODataSdmTranslatorTests
	{
		private static List<Person> _people = new List<Person>
		{
			new Person("Person 001", "Andi Tamer", 105)
			{
				Aliases =
				{
					"Agent Andi",
					"Tamer the Great",
				},
				ExternalIdentifiers =
				{
					new ExternalIdentifier
					{
						ExternalID = "e001",
						ExternalName = "EXT Andi Tamer",
					},
				},
			},
			new Person("Person 002", "John Doe", 35)
			{
				Aliases =
				{
					"JD",
					"Johnny",
				},
				ExternalIdentifiers =
				{
					new ExternalIdentifier
					{
						ExternalID = "p002",
						ExternalName = "EXT John Doe",
					},
				},
			},
			new Person("Person 003", "Fiber Squad", 3)
			{
				Aliases =
				{
					"Fiber",
					"The Squad",
				},
				ExternalIdentifiers =
				{
					new ExternalIdentifier
					{
						ExternalID = "p003",
						ExternalName = "SQUAD Fiber",
					},
				},
			},
		};

		[TestMethod]
		public void Empty_Filter()
		{
			// Arrange
			var expectedFilter = new TRUEFilterElement<Person>();
			var odataFilter = "";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void String_Equal()
		{
			// Arrange
			var expectedFilter = PersonExposers.Name.Equal("Andi Tamer");
			var odataFilter = "Name eq 'Andi Tamer'";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void String_NotEqual()
		{
			// Arrange
			var expectedFilter = PersonExposers.Name.NotEqual("Andi Tamer");
			var odataFilter = "Name ne 'Andi Tamer'";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void String_Contains()
		{
			// Arrange
			var expectedFilter = PersonExposers.Name.Contains("Andi");
			var odataFilter = "contains(Name, 'Andi')";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void Number_GreaterThan()
		{
			// Arrange
			var expectedFilter = PersonExposers.Age.GreaterThan(20);
			var odataFilter = "Age gt 20";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void Number_GreaterThanOrEqual()
		{
			// Arrange
			var expectedFilter = PersonExposers.Age.GreaterThanOrEqual(20);
			var odataFilter = "Age ge 20";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void Number_LessThan()
		{
			// Arrange
			var expectedFilter = PersonExposers.Age.LessThan(20);
			var odataFilter = "Age lt 20";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void Number_LessThanOrEqual()
		{
			// Arrange
			var expectedFilter = PersonExposers.Age.LessThanOrEqual(20);
			var odataFilter = "Age le 20";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void OrderBy_Empty()
		{
			// Arrange
			var expectedQuery = new TRUEFilterElement<Person>().ToQuery();
			var orderBy = "";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var query = translator.TranslateOrderBy(orderBy);

			// Assert
			query.Should().Be(expectedQuery, new QueryEqualityComparer<Person>());
		}

		[TestMethod]
		public void OrderBy_ID()
		{
			// Arrange
			var expectedQuery = new TRUEFilterElement<Person>()
				.ToQuery()
				.OrderBy(PersonExposers.Identifier);
			var orderBy = "Identifier";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var query = translator.TranslateOrderBy(orderBy);

			// Assert
			query.Should().Be(expectedQuery, new QueryEqualityComparer<Person>());
		}

		[TestMethod]
		public void OrderBy_ID_Persons_Descending()
		{
			// Arrange
			var orderByOData = "Identifier desc";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var query = translator.TranslateOrderBy(orderByOData);
			var result = query.ExecuteInMemory(_people);

			// Assert
			Console.WriteLine(query);
			result.Should().BeInDescendingOrder(p => p.Identifier);
		}

		[TestMethod]
		public void StringCollection_Contains()
		{
			// Arrange
			var expectedFilter = PersonExposers.Aliases.Contains("Agent Andi");
			var odataFilter = "contains(Aliases, 'Agent Andi')";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void Collection_Any()
		{
			// Arrange
			var expectedFilter = PersonExposers.ExternalIdentifiers.ExternalName.Equal("EXT Andi Tamer");
			var odataFilter = "ExternalIdentifiers/any(e: e/ExternalName eq 'EXT Andi Tamer')";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}

		[TestMethod]
		public void Collection_Any_With_And()
		{
			// Arrange
			var expectedFilter = new ANDFilterElement<Person>(
				PersonExposers.ExternalIdentifiers.ExternalID.Contains("p"),
				PersonExposers.ExternalIdentifiers.ExternalName.Contains("EXT"));
			var odataFilter = "ExternalIdentifiers/any(e: contains(e/ExternalID, 'p') and contains(e/ExternalName, 'EXT'))";

			// Act
			var translator = new ODataSdmTranslator<Person>();
			var filter = translator.TranslateFilter(odataFilter);

			// Assert
			filter.Should().Be(expectedFilter);
		}
	}
}
