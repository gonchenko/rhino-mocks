using System;
using System.Collections.Generic;
using System.Text;
#if NETFRAMEWORK
using ADODB;
#endif
using Xunit;

namespace Rhino.Mocks.Tests.FieldsProblem
{
#if NETFRAMEWORK
	public class FieldProblem_dyowee
	{
		[Fact]
		public void MockingRecordSet()
		{
			MockRepository mr = new MockRepository();
			Recordset mock = mr.StrictMock<ADODB.Recordset>();
			Assert.NotNull(mock);
			Expect.Call(mock.ActiveConnection).Return("test");
			mr.ReplayAll();
			Assert.Equal("test", mock.ActiveConnection);
			mr.VerifyAll();
		}
	}
#endif
}
