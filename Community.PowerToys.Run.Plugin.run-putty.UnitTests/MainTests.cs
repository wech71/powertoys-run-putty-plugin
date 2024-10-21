using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO.Packaging;
using System.Linq;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.run_putty.UnitTests
{
    [TestClass]
    public class MainTests
    {
        private Main main;


		[TestInitialize]
        public void TestInitialize()
        {
            PackUriHelper.Create(new Uri("reliable://0"));

			main = new Main();
		}

        [TestMethod]
        public void Query_should_return_results()
        {
            var results = main.Query(new("putty"));

            Assert.IsNotNull(results.First());
        }

        [TestMethod]
        public void LoadContextMenus_should_return_results()
        {
            var results = main.LoadContextMenus(new Result { ContextData = "Default" });

            Assert.IsNotNull(results.First());
        }
    }
}
