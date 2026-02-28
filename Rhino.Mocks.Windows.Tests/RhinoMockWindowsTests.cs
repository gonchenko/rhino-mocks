using System;
using System.ServiceModel;
using Rhino.Mocks.Tests.Callbacks;
using Xunit;

namespace Rhino.Mocks.Tests
{
    public class RhinoMockWindowsTests : RhinoMockTests
    {
        [Fact]
        public void CallbackThatThrows()
        {
            demo = (IDemo) mocks.StrictMock(typeof (IDemo));
            demo.VoidNoArgs();
            LastCall.Callback((Delegate) new DelegateDefinations.NoArgsDelegate(ThrowFromCallback));
            mocks.ReplayAll();
            Assert.Throws<AddressAlreadyInUseException>(demo.VoidNoArgs);
        }

        private bool ThrowFromCallback()
        {
            throw new AddressAlreadyInUseException();
        }
    }
}