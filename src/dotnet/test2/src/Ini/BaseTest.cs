using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Css;
using JetBrains.ReSharper.Resources.Shell;
using NUnit.Framework;
using RiderPlugin.UnrealLink.Ini.IniLanguage;

namespace Test.RiderPlugin.UnrealLink.Ini
{
    // [TestFileExtension(IniProjectFileType.Ini_EXTENSION)]
    // public class ParserTests : ParserTestBase<IniLanguage>
    // {
    //     protected override string RelativeTestDataPath => @"Lexing";
    //     
    //     [TestCaseSource("test01")]
    //     public void TestParser(string name) => DoOneTest(name);
    // }

    [TestFixture]
    public class BaseTest
    {
        // Works fine
        [Test]
        public void Test1()
        {
            Assert.AreEqual(1, 1);
        }
        
        // Also works fine
        [Test]
        public void Test2()
        {
            Assert.NotNull(Languages.Instance.GetLanguageByName(CssLanguage.Name));
            Assert.NotNull(CssProjectFileType.Instance);
            var projectFileTypes = Shell.Instance.GetComponent<IProjectFileTypes>();
            Assert.NotNull(projectFileTypes.GetFileType(CssProjectFileType.Name));
        }
        
        // Not working
        // [Test]
        // public void Test3()
        // {
        //     Assert.NotNull(Languages.Instance.GetLanguageByName(IniLanguage.Name));
        //     Assert.NotNull(IniProjectFileType.Instance);
        //     var projectFileTypes = Shell.Instance.GetComponent<IProjectFileTypes>();
        //     Assert.NotNull(projectFileTypes.GetFileType(IniProjectFileType.Name));
        // }
    }
}