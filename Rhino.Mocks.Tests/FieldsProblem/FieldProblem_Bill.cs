using Xunit;
#if NETFRAMEWORK
using Microsoft.Practices.Unity;
#endif

namespace Rhino.Mocks.Tests.FieldsProblem
{
#if NETFRAMEWORK
    public class FieldProblem_Bill
    {
        /// <summary>
        /// From thread:
        /// http://groups.google.com/group/rhinomocks/browse_thread/thread/a22b18618be887ff?hl=en
        /// </summary>
        [Fact]
        public void Should_be_able_to_proxy_IUnityContainer()
        {
            var unity = MockRepository.GenerateMock<IUnityContainer>();
        }
    }
#endif
}
