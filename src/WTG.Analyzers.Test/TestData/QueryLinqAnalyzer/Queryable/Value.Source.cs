using System.Linq;

static class Foo
{
	public static int Count1(IQueryable<Item> items) => items.Count(x => x.Name.StartsWith("Foo"));
	public static bool All1(IQueryable<Item> items) => items.All(x => x.Name.StartsWith("Foo"));
	public static bool Any1(IQueryable<Item> items) => items.Any(x => x.Name.StartsWith("Foo"));
}

sealed class Item
{
	public string Name { get; set; }
}
