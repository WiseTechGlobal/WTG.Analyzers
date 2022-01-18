using System.Collections.Generic;
using System.Linq;

static class Foo
{
	public static IEnumerable<Item> Where1(IQueryable<Item> items) => items.Where(x => x.Name.StartsWith("Foo"));
	public static IEnumerable<Tag> Select1(IQueryable<Item> items) => items.Select(x => x.Tag);
	public static IEnumerable<Item> OrderBy1(IQueryable<Item> items)=>items.OrderBy(x=>x.Name);
}

sealed class Item
{
	public string Name { get; set; }
	public Tag Tag { get; set; }
}

sealed class Tag
{
	public string Name { get; set; }
}
