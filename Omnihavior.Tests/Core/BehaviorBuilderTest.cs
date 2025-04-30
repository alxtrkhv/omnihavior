using Omnihavior.Core;
using Omnihavior.Tests.Mocks;

namespace Omnihavior.Tests.Core;

[TestFixture]
public class BehaviorBuilderTests
{
  [Test]
  public void Create_CreatesBuilderOfRequestedType()
  {
    var builder = Builder.Create<TestInput>();

    Assert.That(builder, Is.InstanceOf<BehaviourBuilder<TestInput>>());
  }

  [Test]
  public void InputType_ReturnsInputType()
  {
    var builder = Builder.Create<TestInput>();

    Assert.That(builder.InputType, Is.EqualTo(typeof(TestInput)));
  }

  [Test]
  public void Default_ReturnsDefaultBuilder()
  {
    var builder1 = Builder.Default<TestInput>();
    var builder2 = Builder.Default<TestInput>();

    Assert.That(builder1, Is.SameAs(builder2));
  }
}
