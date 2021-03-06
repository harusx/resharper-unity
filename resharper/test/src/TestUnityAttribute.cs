using System;
using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Plugins.Unity.ProjectModel.Properties.Flavours;
using JetBrains.ReSharper.TestFramework;
using NuGet;
using PlatformID = JetBrains.Application.platforms.PlatformID;

namespace JetBrains.ReSharper.Plugins.Unity.Tests
{
    public enum UnityVersion
    {
        Unity54,
        Unity55,
        Unity56,
        Unity20171,
        Unity20172
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TestUnityAttribute : TestPackagesAttribute, ITestFlavoursProvider, ITestPlatformProvider, ITestFileExtensionProvider, ICustomProjectPropertyAttribute
    {
        private readonly UnityVersion myVersion;

        public TestUnityAttribute() : this(UnityVersion.Unity20172)
        {
        }

        public TestUnityAttribute(UnityVersion version)
        {
            myVersion = version;
        }

        public bool IncludeNetworking { get; set; }

        public override IEnumerable<PackageDependency> GetPackages(PlatformID platformID)
        {
            // There isn't an official nuget for Unity, sadly, so add this feed to test/data/nuget.config
            // <add key="unity-testlibs" value="https://myget.org/F/resharper-unity/api/v2/" />
            var version = ToVersionString(myVersion);
            yield return ParsePackageDependency($"resharper-unity.testlibs/{version}");
            if (IncludeNetworking)
            {
                if (myVersion == UnityVersion.Unity54)
                    throw new InvalidOperationException("Network libs not available for Unity 5.4");
                yield return ParsePackageDependency($"resharper-unity.testlibs.networking/{version}");
            }
            foreach (var package in base.GetPackages(platformID))
                yield return package;
        }

        public Guid[] GetProjectTypeGuids()
        {
            return new[]
            {
                UnityProjectFlavor.UnityProjectFlavorGuid
            };
        }

        public PlatformID GetPlatformID()
        {
            return PlatformID.CreateFromName(".NETFrameWork", new Version(4, 0));
        }

        public string Extension => CSharpProjectFileType.CS_EXTENSION;

        public string DefineConstants
        {
            get
            {
                switch (myVersion)
                {
                    case UnityVersion.Unity54: return "UNITY_5_4";
                    case UnityVersion.Unity55: return "UNITY_5_5";
                    case UnityVersion.Unity56: return "UNITY_5_6";
                    case UnityVersion.Unity20171: return "UNITY_2017_1";
                    case UnityVersion.Unity20172: return "UNITY_2017_2";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private string ToVersionString(UnityVersion unityVersion)
        {
            switch (unityVersion)
            {
                case UnityVersion.Unity54: return "5.4.0";
                case UnityVersion.Unity55: return "5.5.0";
                case UnityVersion.Unity56: return "5.6.0";
                case UnityVersion.Unity20171: return "2017.1.0";
                case UnityVersion.Unity20172: return "2017.2.0";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unityVersion), unityVersion, null);
            }
        }

        public string PropertyName => "DefineConstants";
        public string PropertyValue => DefineConstants;
    }
}