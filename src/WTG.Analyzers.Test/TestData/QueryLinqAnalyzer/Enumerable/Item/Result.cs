using System.Linq;

static class Foo
{
	public static Item First1(IQueryable<Item> items) => items.AsEnumerable().First(Filter);
	public static Item Single1(IQueryable<Item> items) => items.AsEnumerable().Single(Filter);
	public static Item FirstOrDefault1(IQueryable<Item> items) => items.AsEnumerable().FirstOrDefault(Filter);
	public static Item SingleOrDefault1(IQueryable<Item> items) => items.AsEnumerable().SingleOrDefault(Filter);

	static bool Filter(Item x) => x.Name.StartsWith("Foo");
}

sealed class Item
{
	public string Name { get; set; }
}
