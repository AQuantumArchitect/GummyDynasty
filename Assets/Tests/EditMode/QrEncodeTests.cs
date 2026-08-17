using GummyDynasty.Core;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class QrEncodeTests
    {
        [Test]
        public void Url_MakesSquareWithFinders()
        {
            var g = QrEncode.Encode("http://192.168.1.10:8787/");
            Assert.IsNotNull(g);
            var n = g.GetLength(0);
            Assert.AreEqual(n, g.GetLength(1));
            Assert.IsTrue(n == QrEncode.Version2Size || n == QrEncode.Version3Size);
            Assert.IsTrue(g[0, 0]);
            Assert.IsTrue(g[6, 0]);
            Assert.IsTrue(g[0, 6]);
            Assert.IsTrue(g[n - 1, 0]);
            Assert.IsTrue(g[0, n - 1]);
        }

        [Test]
        public void DifferentUrls_Differ()
        {
            var a = QrEncode.Encode("http://192.168.1.10:8787/");
            var b = QrEncode.Encode("http://10.0.0.4:8787/");
            var same = true;
            var n = a.GetLength(0);
            for (var y = 0; y < n && same; y++)
            for (var x = 0; x < n && same; x++)
                if (a[x, y] != b[x, y])
                    same = false;
            Assert.IsFalse(same);
        }
    }
}
