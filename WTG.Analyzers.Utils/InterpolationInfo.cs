using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public readonly struct InterpolationInfo
	{
		public static InterpolationInfo Extract(SemanticModel semanticModel, InterpolatedStringExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var builder = new StringBuilder();
			var expressionCount = 0;

			foreach (var section in expression.Contents)
			{
				switch (section.Kind())
				{
					case SyntaxKind.InterpolatedStringText:
						builder.Append(((InterpolatedStringTextSyntax)section).TextToken.ValueText);
						break;

					case SyntaxKind.Interpolation:
						var interpolation = (InterpolationSyntax)section;

						builder.Append('{').Append(expressionCount.ToString(CultureInfo.InvariantCulture));

						if (interpolation.AlignmentClause != null)
						{
							builder.Append(',');

							var value = semanticModel.GetConstantValue(interpolation.AlignmentClause.Value, cancellationToken);
							builder.Append(value.HasValue ? value.Value : interpolation.AlignmentClause.Value.ToString());
						}

						if (interpolation.FormatClause != null)
						{
							builder.Append(':').Append(interpolation.FormatClause.FormatStringToken.Text);
						}

						builder.Append('}');
						expressionCount++;
						break;
				}
			}

			if (expressionCount == 0)
			{
				return new InterpolationInfo(builder.ToString(), Array.Empty<ExpressionSyntax>());
			}

			var expressions = new ExpressionSyntax[expressionCount];
			var i = 0;

			foreach (var section in expression.Contents)
			{
				if (section.IsKind(SyntaxKind.Interpolation))
				{
					var interpolation = (InterpolationSyntax)section;

					expressions[i++] = interpolation.Expression;
				}
			}

			return new InterpolationInfo(builder.ToString(), expressions);
		}

		public InterpolationInfo(string format, ExpressionSyntax[] expressions)
		{
			Format = format;
			Expressions = expressions;
		}

		public string Format { get; }
		public ExpressionSyntax[] Expressions { get; }
	}
}
