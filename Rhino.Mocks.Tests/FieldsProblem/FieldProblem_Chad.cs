using System;
using Xunit;

namespace Rhino.Mocks.Tests.FieldsProblem
{
	
	public class FieldProblem_Chad
	{
		[Fact]
		public void SetupResult_For_writeable_property_on_stub_should_be_ignored()
		{
			MockRepository mocks = new MockRepository();
			TestClass test = mocks.Stub<TestClass>();
			SetupResult.For(test.ReadOnly).Return("foo");

			Assert.Throws<InvalidOperationException>(() => SetupResult.For(test.ReadWrite).PropertyBehavior());
		}
		public class TestClass
		{
			public virtual string ReadOnly { get { return ""; } }
			public virtual string ReadWrite { get { return null; } set { } }
		}
	}
}