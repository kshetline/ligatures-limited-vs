using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace LigaturesLimited
{
  // To get loaded into VS, the package must be referred by <Asset Type="Microsoft.VisualStudio.VsPackage" ...> in .vsixmanifest file.
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  [Guid(LigaturesLimitedPackage.PackageGuidString)]
  public sealed class LigaturesLimitedPackage : AsyncPackage
  {
    public const string PackageGuidString = "20319ebb-c940-4302-82e5-e02433def57d";

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
    }
  }
}
