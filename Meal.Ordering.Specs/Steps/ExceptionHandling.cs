using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Meal.Ordering.Specs.Steps;

[Binding]
public class ExceptionHandling
{
    private readonly ExceptionContext _exceptionContext;

    public ExceptionHandling(ExceptionContext exceptionContext)
    {
        _exceptionContext = exceptionContext;
    }

    [Then(@"the error message is '([^']*)'")]
    [Then(@"the error message is ""([^""]*)""")]
    public void ThenTheErrorMessageIs(string expectedMessage)
    {
        Assert.That(_exceptionContext.LastException, Is.Not.Null);
        Assert.That(_exceptionContext.LastException!.Message, Is.EqualTo(expectedMessage));
        _exceptionContext.LastException = null;
    }


    [AfterScenario]
    public void ClearException()
    {
        if (_exceptionContext.LastException != null)
            throw _exceptionContext.LastException;
    }
}