using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skybrud.Umbraco.BorgerDk.Models;

namespace UnitTestProject1 {

    [TestClass]
    public class UnitTest1 {

        [TestMethod]
        public void TestMethod1() {

            string json = "{\"id\":0,\"url\":\"\",\"selected\":[],\"customTitle\":{\"type\":\"article\",\"value\":\"\"}}";

            BorgerDkArticleSelection selection = BorgerDkArticleSelection.Deserialize(json);

            Assert.AreEqual(0, selection.Id);
            Assert.AreEqual(string.Empty, selection.Url);
            Assert.AreEqual(0, selection.Selected.Length);
            Assert.IsNull(selection.Municipality);
            Assert.IsNull(selection.Municipality);

        }

    }

}