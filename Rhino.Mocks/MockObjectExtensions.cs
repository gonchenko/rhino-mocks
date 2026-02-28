using Rhino.Mocks.Interfaces;
#if NETFRAMEWORK
using Rhino.Mocks.Impl.RemotingMock;
#endif

namespace Rhino.Mocks
{
	internal static class MockObjectExtensions
	{
		internal static IMockedObject AsMockObject(this object mockedInstance)
		{
			IMockedObject mockedObj = mockedInstance.AsMockObjectOrNull();
			if (mockedObj == null)
				throw new System.InvalidOperationException("The object '" + mockedInstance +
				                                          "' is not a mocked object.");
			return mockedObj;
		}

		internal static IMockedObject AsMockObjectOrNull(this object mockedInstance)
		{
			System.Delegate mockedDelegate = mockedInstance as System.Delegate;

			if (mockedDelegate != null)
			{
				mockedInstance = mockedDelegate.Target;
			}

			// must be careful not to call any methods on mocked objects,
			// or it may cause infinite recursion
			if (mockedInstance is IMockedObject)
			{
				return (IMockedObject)mockedInstance;
			}

#if NETFRAMEWORK
			if (RemotingMockGenerator.IsRemotingProxy(mockedInstance))
			{
				return RemotingMockGenerator.GetMockedObjectFromProxy(mockedInstance);
			}
#endif

			return null;
		}
	}
}
