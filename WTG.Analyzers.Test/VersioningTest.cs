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
			var packageDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "NuGet");
			var packages = Directory.GetFiles(packageDirectory, "*.nupkg", SearchOption.TopDirectoryOnly);

			Assert.That(packages.Length, Is.EqualTo(1), "Sanity check - Exactly one NuGet package should have been created.");

			var package = packages[0];

			var analyzersVersion = GetAssemblyVersion("WTG.Analyzers");
			var expectedPackageName = FormattableString.Invariant($"WTG.Analyzers.{analyzersVersion.ToString(fieldCount: 3)}.nupkg");
			Assert.That(Path.GetFileName(package), Is.EqualTo(expectedPackageName));
		}

		static Version GetAssemblyVersion(string assemblyName)
			=> Assembly.Load(assemblyName).GetName().Version;
	}
}
