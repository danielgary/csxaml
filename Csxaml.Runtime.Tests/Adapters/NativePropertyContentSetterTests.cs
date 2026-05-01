namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class NativePropertyContentSetterTests
{
    [TestMethod]
    public void Set_SinglePropertyContent_SkipsSameInstance()
    {
        var target = new CountingContentTarget();
        var child = new object();

        NativePropertyContentSetter.Set(target, "Content", [child]);
        NativePropertyContentSetter.Set(target, "Content", [child]);

        Assert.AreEqual(1, target.SetCount);
        Assert.AreSame(child, target.Content);
    }

    private sealed class CountingContentTarget
    {
        private object? _content;

        public int SetCount { get; private set; }

        public object? Content
        {
            get => _content;
            set
            {
                SetCount++;
                _content = value;
            }
        }
    }
}
