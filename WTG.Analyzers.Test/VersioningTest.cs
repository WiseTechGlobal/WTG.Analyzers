using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace WTG.Analyzers.Test
{
	class VersioningTest
	{
		[Test]
		public void AssemblyMajorMinorVersionMatchesRoslynMajorMinorVersion()
		{
			var analyzersVersion = GetAssemblyVersion("WTG.Analyzers");
			var roslynVersion = GetAssemblyVersion("Microsoft.CodeAnalysis");

			Assert.Multiple(() =>
			{
				Assert.That(analyzersVersion.Major, Is.EqualTo(roslynVersion.Major), nameof(Version.Major));
				Assert.That(analyzersVersion.Minor, Is.EqualTo(roslynVersion.Minor), nameof(Version.Minor));
			});
		}

		[Test]
		public void AssemblyVersionMatchesNuGetPackageVersion()
		{
			var analyzersVersion = GetAssemblyVersion("WTG.Analyzers");
			var packageDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "..");
			var packages = Directory.GetFiles(packageDirectory, "*.nupkg", SearchOption.TopDirectoryOnly);

			for (var i = 0; i < packages.Length; i++)
			{
				packages[i] = Path.GetFileName(packages[i]);
			}

			var ver = analyzersVersion.ToString(fieldCount: 3);

			var expected = new[]
			{
				FormattableString.Invariant($"WTG.Analyzers.{ver}.nupkg"),
				FormattableString.Invariant($"WTG.Analyzers.TestFramework.{ver}.nupkg"),
			};

			Assert.That(packages, Is.EquivalentTo(expected));
		}

		static Version GetAssemblyVersion(string assemblyName) => Assembly.Load(assemblyName).GetName().Version;
	}
}
