using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PMXEPlugin536.MaskImageToEdge.Tests {
    [TestClass]
    public class Normalize_float_between_0_and_1_Test {

        [TestMethod]
        public void Normalize_simple_Test() {
            var privateType = new PrivateType(typeof(Runner));
            var normarizedVal = privateType.InvokeStatic("normalize", 0.25f);
            Assert.AreEqual(0.25f, normarizedVal);
        }

        [TestMethod]
        public void Normalize_over_1_Test() {
            var privateType = new PrivateType(typeof(Runner));
            var normarizedVal = privateType.InvokeStatic("normalize", 1.75f);
            Assert.AreEqual(0.75f, normarizedVal);
        }

        [TestMethod]
        public void Normalize_negative_Test() {
            var privateType = new PrivateType(typeof(Runner));
            var normarizedVal = privateType.InvokeStatic("normalize", -1.25f);
            Assert.AreEqual(0.75f, normarizedVal);
        }

    }
}
