using System.Collections.Generic;
using System.Linq;

public class ViewModel
{
	public IEnumerable<ViewModel> Items { get; set; }
}

public class Bob
{
	public void Method()
	{
		var viewModel = new ViewModel();

		viewModel.Items.Prepend<object>(viewModel);
		Enumerable.Prepend<object>(viewModel.Items, viewModel);
		viewModel.Items.Append<object>(viewModel);
		Enumerable.Append<object>(viewModel.Items, viewModel);
	}
}
