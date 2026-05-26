using System.Collections.Generic;
using System.Linq;

public class Bob
{
	public void Method()
	{
		var viewModel = new ViewModel();

		new object[] { viewModel }.Concat(viewModel.Items);
		Enumerable.Concat(new object[] { viewModel }, viewModel.Items);
		viewModel.Items.Concat(new object[] { viewModel });
		Enumerable.Concat(viewModel.Items, new object[] { viewModel });
	}
}

public class ViewModel
{
	public IEnumerable<ViewModel> Items { get; set; }
}
