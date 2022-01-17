using System.Linq;

static class Foo
{
	public static int Count1(IQueryable<Item> items) => items.Count(Filter);
	public static bool All1(IQueryable<Item> items) => items.All(Filter);
	public static bool Any1(IQueryable<Item> items) => items.Any(Filter);

	static bool Filter(Item x) => x.Name.StartsWith("Foo");
}

sealed class Item
{
	public string Name { get; set; }
}
