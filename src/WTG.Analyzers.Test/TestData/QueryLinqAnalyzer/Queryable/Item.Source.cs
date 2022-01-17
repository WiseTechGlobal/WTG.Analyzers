using System.Linq;

static class Foo
{
	public static Item First1(IQueryable<Item> items) => items.First(x => x.Name.StartsWith("Foo"));
	public static Item Single1(IQueryable<Item> items) => items.Single(x => x.Name.StartsWith("Foo"));
	public static Item FirstOrDefault1(IQueryable<Item> items) => items.FirstOrDefault(x => x.Name.StartsWith("Foo"));
	public static Item SingleOrDefault1(IQueryable<Item> items) => items.SingleOrDefault(x => x.Name.StartsWith("Foo"));
}

sealed class Item
{
	public string Name { get; set; }
}
