namespace Rhino.Mocks.Impl.RemotingMock
{
    using System;
    using Rhino.Mocks.Interfaces;
    using Castle.DynamicProxy;

    /// <summary>
    /// Generates remoting proxies and provides utility functions
    /// </summary>
#if !NETFRAMEWORK
    internal class RemotingMockGenerator
    {
        ///<summary>
        /// Create the proxy using remoting
        ///</summary>
        public object CreateRemotingMock(Type type, IInterceptor interceptor, IMockedObject mockedObject)
        {
            // Remoting unavailable on Core; caller should avoid invoking this method.
            throw new PlatformNotSupportedException("Remoting is not supported on .NET Core");
        }

        /// <summary>
        /// Check whether an object is a transparent proxy with a RemotingProxy behind it
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>true if the object is a transparent proxy with a RemotingProxy instance behind it, false otherwise</returns>
        /// <remarks>This is called from various helpers; on Core we simply report false rather than throw.</remarks>
        public static bool IsRemotingProxy(object obj)
        {
            // never treat anything as a remoting proxy on Core
            return false;
        }

        /// <summary>
        /// Retrieve a mocked object from a transparent proxy
        /// </summary>
        /// <param name="proxy">Transparent proxy with a RemotingProxy instance behind it</param>
        /// <returns>Mocked object associated with the proxy, or null</returns>
        public static IMockedObject GetMockedObjectFromProxy(object proxy)
        {
            // no remoting proxies exist
            return null;
        }
    }
#else
    internal class RemotingMockGenerator
    {
        ///<summary>
        /// Create the proxy using remoting
        ///</summary>
        public object CreateRemotingMock(Type type, IInterceptor interceptor, IMockedObject mockedObject)
        {
            if (type.IsInterface == false && !typeof(MarshalByRefObject).IsAssignableFrom(type))
            {
                throw new InvalidCastException(
                    String.Format("Cannot create remoting proxy. '{0}' is not derived from MarshalByRefObject", type.Name));
            }

            return new RemotingProxy(type, interceptor, mockedObject).GetTransparentProxy();
        }

        /// <summary>
        /// Check whether an object is a transparent proxy with a RemotingProxy behind it
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>true if the object is a transparent proxy with a RemotingProxy instance behind it, false otherwise</returns>
        /// <remarks>We use Equals() method to communicate with the real proxy behind the object.
        /// See IRemotingProxyOperation for more details</remarks>
        public static bool IsRemotingProxy(object obj)
        {
            if (obj == null) return false;
            RemotingProxyDetector detector = new RemotingProxyDetector();
            obj.Equals(detector);
            return detector.Detected;
        }

        /// <summary>
        /// Retrieve a mocked object from a transparent proxy
        /// </summary>
        /// <param name="proxy">Transparent proxy with a RemotingProxy instance behind it</param>
        /// <returns>Mocked object associated with the proxy</returns>
        /// <remarks>We use Equals() method to communicate with the real proxy behind the object.
        /// See IRemotingProxyOperation for more details</remarks>
        public static IMockedObject GetMockedObjectFromProxy(object proxy)
        {
            if (proxy == null) return null;
            RemotingProxyMockedObjectGetter getter = new RemotingProxyMockedObjectGetter();
            proxy.Equals(getter);
            return getter.MockedObject;
        }
    }
#endif
}
