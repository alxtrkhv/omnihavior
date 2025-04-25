using Omnihavior.Core;
using Omnihavior.Tests.Mocks;

namespace Omnihavior.Tests.Core;

public class LambaEvaluationTests
{
  [Test]
  public void Evaluate_ReturnsResultOfProvidedFunction()
  {
    var expectedResult = 42.0f;
    var lambdaEvaluation = new LambdaEvaluation<TestInputFloat>(input => input.Value * 2f);

    var actualResult = lambdaEvaluation.Evaluate(new(21f));

    Assert.That(actualResult, Is.EqualTo(expectedResult));
  }
}
