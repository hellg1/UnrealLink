using System;
using System.IO;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: RequiresThread(System.Threading.ApartmentState.STA)]

namespace Test.RiderPlugin.UnrealLink
{
    [ZoneDefinition]
    public interface IIniTestZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>
    {
    }

    [ZoneActivator]
    class PsiFeatureTestZoneActivator : IActivate<PsiFeatureTestZone>
    {
        public bool ActivatorEnabled()
        {
            return true;
        }
    }

    [SetUpFixture]
    public class TestEnvironment : ExtensionTestEnvironmentAssembly<IIniTestZone>
    {
    }
}