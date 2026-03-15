using Xunit;
using Rhino.Mocks.Exceptions;

namespace Rhino.Mocks.Tests.FieldsProblem
{
	
	public class FieldProblem_Robert
	{
		public interface IView
		{
			void RedrawDisplay(object something);
		}

		[Fact]
		public void CorrectResultForExpectedWhenUsingTimes()
		{
			MockRepository mocks = new MockRepository();
			IView view = mocks.StrictMock<IView>();
			using (mocks.Record())
			{
				view.RedrawDisplay(null);
				LastCall.Repeat.Times(4).IgnoreArguments();
			}
            Assert.Throws<ExpectationViolationException>(() =>
            {
                using (mocks.Playback())
                {
                    for (int i = 0; i < 5; i++)
                    {
                        view.RedrawDisplay("blah");
                    }
                }
            });
			
		}

		[Fact]
		public void CorrectResultForExpectedWhenUsingTimesWithRange()
		{
			MockRepository mocks = new MockRepository();
			IView view = mocks.StrictMock<IView>();
			using (mocks.Record())
			{
				view.RedrawDisplay(null);
				LastCall.Repeat.Times(3,4).IgnoreArguments();
			}
            Assert.Throws<ExpectationViolationException>(() =>
            {
                using (mocks.Playback())
                {
                    for (int i = 0; i < 5; i++)
                    {
                        view.RedrawDisplay("blah");
                    }
                }
            });
			
		}
	}
}