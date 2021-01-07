﻿#region license
// Copyright (c) 2005 - 2007 Ayende Rahien (ayende@ayende.com)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Ayende Rahien nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion


using System;
using Xunit;
using Rhino.Mocks.Exceptions;
using Rhino.Mocks.Interfaces;

namespace Rhino.Mocks.Tests
{
	
	public class MockRepositoryTests
	{
		private MockRepository mocks;
		private IDemo demo;

		public MockRepositoryTests()
		{
			mocks = new MockRepository();
			demo = this.mocks.StrictMock(typeof(IDemo)) as IDemo;
		}

		[Fact]
		public void CreatesNewMockObject()
		{
			Assert.NotNull(demo);
		}

		[Fact]
		public void CallMethodOnMockObject()
		{
			demo.ReturnStringNoArgs();
		}

		[Fact]
		public void RecordWithBadReplayCauseException()
		{
			demo.ReturnStringNoArgs();
			LastCall.On(demo).Return(null);
			mocks.Replay(demo);
            Assert.Throws<ExpectationViolationException>(
                () => mocks.Verify(demo));
		}

		[Fact]
		public void RecordTwoMethodsButReplayOneCauseException()
		{
			demo.ReturnStringNoArgs();
			LastCall.On(demo).Return(null).Repeat.Twice();
			mocks.Replay(demo);
			demo.ReturnStringNoArgs();
            Assert.Throws<ExpectationViolationException>(
                () => mocks.Verify(demo));
		}

		[Fact]
		public void CallingReplayOnNonMockThrows()
		{
			MockRepository mocks = new MockRepository();
            Assert.Throws<ObjectNotMockFromThisRepositoryException>(
                () => mocks.Replay(new object()));
		}

		[Fact]
		public void CallingVerifyOnNonMockThrows()
		{
			MockRepository mocks = new MockRepository();
            Assert.Throws<ObjectNotMockFromThisRepositoryException>(
                () => mocks.Verify(new object()));
		}

		[Fact]
		public void TryingToReplayMockMoreThanOnceThrows()
		{
			mocks.Replay(demo);
            Assert.Throws<InvalidOperationException>(
                () => mocks.Replay(demo));
		}

		[Fact]
		public void CallingReplayAndThenReplayAll()
		{
			mocks.Replay(demo);
			mocks.ReplayAll();
		}


		[Fact]
		public void CallingVerifyAndThenVerifyAll()
		{
			mocks.ReplayAll();
			mocks.Verify(demo);
			mocks.VerifyAll();
		}

		[Fact]
        public void CallingVerifyWithoutReplayFirstCauseException()
		{
            Assert.Throws<InvalidOperationException>(
                () => mocks.Verify(demo));
		}

		[Fact]
		public void UsingVerifiedObjectThrows()
		{
			mocks.Replay(demo);
			mocks.Verify(demo);
            Assert.Throws<InvalidOperationException>(
                () => demo.ReturnIntNoArgs());
		}


		[Fact]
		public void CallingLastMethodOptionsOnReplay()
		{
			demo.VoidNoArgs();
			mocks.Replay(demo);
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo));
		}

		[Fact]
		public void NotClosingMethodBeforeReplaying()
		{
			demo.StringArgString("");
            Assert.Throws<InvalidOperationException>(
                () => mocks.Replay(demo));
		}

		[Fact]
		public void GetmocksFromProxy()
		{
			IMockedObject mockedObj = demo as IMockedObject;
			Assert.NotNull(mockedObj);
			MockRepository MockRepository = mockedObj.Repository;
			Assert.NotNull(MockRepository);
			Assert.Same(mocks, MockRepository);
		}

		[Fact]
		public void CallingLastCallWithoutHavingLastCallThrows()
		{
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo));
		}

		[Fact]
		public void SetReturnValue()
		{
			demo.ReturnStringNoArgs();
			string retVal = "Ayende";
			LastCall.On(demo).Return(retVal);
			mocks.Replay(demo);
			Assert.Equal(retVal, demo.ReturnStringNoArgs());
			mocks.Verify(demo);
		}

		[Fact]
		public void SetReturnValueAndNumberOfRepeats()
		{
			demo.ReturnStringNoArgs();
			string retVal = "Ayende";
			LastCall.On(demo).Return(retVal).Repeat.Twice();
			mocks.Replay(demo);
			Assert.Equal(retVal, demo.ReturnStringNoArgs());
			Assert.Equal(retVal, demo.ReturnStringNoArgs());
			mocks.Verify(demo);
		}

		[Fact]
		public void SetMethodToThrow()
		{
			demo.VoidStringArg("test");
			LastCall.On(demo).Throw(new ArgumentException("Reserved value, must be zero"));
			mocks.Replay(demo);
            Assert.Throws<ArgumentException>(() => demo.VoidStringArg("test"));
		}

		[Fact]
		public void SettingMethodToThrowTwice()
		{
			demo.VoidStringArg("test");
			string exceptionMessage = "Reserved value, must be zero";
			LastCall.On(demo).Throw(new ArgumentException(exceptionMessage)).Repeat.Twice();
			mocks.Replay(demo);
			for (int i = 0; i < 2; i++)
			{
				try
				{
					demo.VoidStringArg("test");
					Assert.False(true, "Expected exception");
				}
				catch (ArgumentException e)
				{
					Assert.Equal(exceptionMessage, e.Message);
				}
			}
		}

		[Fact]
		public void ReturnningValueType()
		{
			demo.ReturnIntNoArgs();
			LastCall.On(demo).Return(2);
			mocks.Replay(demo);
			Assert.Equal(2, demo.ReturnIntNoArgs());
		}

		[Fact]
		public void CallingSecondMethodWithoutSetupRequiredInfoOnFirstOne()
		{
			demo.ReturnIntNoArgs();
            Assert.Throws<InvalidOperationException>(
                () => demo.ReturnIntNoArgs());
		}

		[Fact]
		public void TryingToSetUnrelatedTypeAsReturnValueThrows()
		{
			demo.ReturnIntNoArgs();
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo).Return(new DateTime()));
		}

		[Fact]
		public void ReturnNullForValueType()
		{
			demo.ReturnIntNoArgs();
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo).Return(null));
		}

		[Fact]
		public void ReturnValueForVoidMethod()
		{
			demo.VoidNoArgs();
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo).Return(3));
		}

		[Fact]
		public void ReturnDerivedType()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Return(DemoEnum.Demo);
		}

		[Fact]
		public void SetExceptionAndThenSetReturn()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Throw(new Exception());
			demo.EnumNoArgs();
			LastCall.On(demo).Return(DemoEnum.Demo);
			mocks.Replay(demo);
			try
			{
				demo.EnumNoArgs();
				Assert.False(true, "Expected exception");
			}
			catch (Exception)
			{
			}
			DemoEnum d = DemoEnum.NonDemo;
			d = (DemoEnum)demo.EnumNoArgs();
			Assert.Equal(d, DemoEnum.Demo);
		}

		[Fact]
		public void SetReturnValueAndExceptionThrows()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Throw(new Exception());
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo).Return(DemoEnum.Demo));
		}

		[Fact]
		public void SetExceptionAndThenThrows()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Throw(new Exception());
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo).Return(DemoEnum.Demo));
		}

		[Fact]
		public void SetTwoReturnValues()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Return(DemoEnum.Demo);
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo).Return(DemoEnum.Demo));
		}

		[Fact]
		public void SetTwoExceptions()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Throw(new Exception());
            Assert.Throws<InvalidOperationException>(
                () => LastCall.On(demo).Throw(new Exception()));

		}

		[Fact]
		public void ExpectMethodOnce()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Return(DemoEnum.NonDemo).Repeat.Once();
			mocks.Replay(demo);
			DemoEnum d = (DemoEnum)demo.EnumNoArgs();
			Assert.Equal(d, DemoEnum.NonDemo);
			try
			{
				demo.EnumNoArgs();
				Assert.False(true, "Expected exception");
			}
			catch (ExpectationViolationException e)
			{
				Assert.Equal("IDemo.EnumNoArgs(); Expected #1, Actual #2.", e.Message);
			}
		}

		[Fact]
		public void ExpectMethodAlways()
		{
			demo.EnumNoArgs();
			LastCall.On(demo).Return(DemoEnum.NonDemo).Repeat.Any();
			mocks.Replay(demo);
			demo.EnumNoArgs();
			demo.EnumNoArgs();
			demo.EnumNoArgs();
			mocks.Verify(demo);
		}

		[Fact]
		public void DifferentArgumentsCauseException()
		{
			demo.VoidStringArg("Hello");
			mocks.Replay(demo);
            Assert.Throws<ExpectationViolationException>(
                () => demo.VoidStringArg("World"));
		}

		[Fact]
		public void VerifyingArguments()
		{
			demo.VoidStringArg("Hello");
			mocks.Replay(demo);
			demo.VoidStringArg("Hello");
			mocks.Verify(demo);
		}

		[Fact]
		public void IgnoreArgument()
		{
			demo.VoidStringArg("Hello");
			LastCall.On(demo).IgnoreArguments();
			mocks.Replay(demo);
			demo.VoidStringArg("World");
			mocks.Verify(demo);
		}

		[Fact]
		public void IgnoreArgsAndReturnValue()
		{
			demo.StringArgString("Hello");
			string objToReturn = "World";
			LastCall.On(demo).IgnoreArguments().Repeat.Twice().Return(objToReturn);
			mocks.Replay(demo);
			Assert.Equal(objToReturn, demo.StringArgString("foo"));
			Assert.Equal(objToReturn, demo.StringArgString("bar"));
			mocks.Verify(demo);
		}

		[Fact]
		public void RepeatThreeTimes()
		{
			demo.StringArgString("Hello");
			string objToReturn = "World";
			LastCall.On(demo).IgnoreArguments().Repeat.Times(3).Return(objToReturn);
			mocks.Replay(demo);
			Assert.Equal(objToReturn, demo.StringArgString("foo"));
			Assert.Equal(objToReturn, demo.StringArgString("bar"));
			Assert.Equal(objToReturn, demo.StringArgString("bar"));
			mocks.Verify(demo);
		}

		[Fact]
		public void RepeatOneToThreeTimes()
		{
			demo.StringArgString("Hello");
			string objToReturn = "World";
			LastCall.On(demo).IgnoreArguments().Repeat.Times(1, 3).Return(objToReturn);
			mocks.Replay(demo);
			Assert.Equal(objToReturn, demo.StringArgString("foo"));
			Assert.Equal(objToReturn, demo.StringArgString("bar"));
			mocks.Verify(demo);
		}

		[Fact]
		public void ThrowingExceptions()
		{
			demo.StringArgString("Ayende");
			LastCall.On(demo).Throw(new Exception("Ugh! It's alive!")).IgnoreArguments();
			mocks.Replay(demo);
            Assert.Throws<Exception>(() => demo.StringArgString(null));
		}


		[Fact]
		public void ThrowingExceptionsWhenOrdered()
		{
			using (mocks.Ordered())
			{
				demo.StringArgString("Ayende");
				LastCall.On(demo).Throw(new Exception("Ugh! It's alive!")).IgnoreArguments();
			}
			mocks.Replay(demo);
            Assert.Throws<Exception>(() => demo.StringArgString(null));
		}

		[Fact]
		public void ExpectationExceptionWhileUsingDisposableThrowTheCorrectExpectation()
		{
			mocks.Replay(demo);
            Assert.Throws<ExpectationViolationException>(
                () => demo.VoidNoArgs());
		}

		[Fact]
		public void MockObjectThrowsForUnexpectedCall()
		{
			MockRepository mocks = new MockRepository();
			IDemo demo = (IDemo)mocks.StrictMock(typeof(IDemo));
			mocks.ReplayAll();
            Assert.Throws<ExpectationViolationException>(
                () => demo.VoidNoArgs());
		}



		[Fact]
		public void MockObjectThrowsForUnexpectedCall_WhenVerified_IfFirstExceptionWasCaught()
		{
			MockRepository mocks = new MockRepository();
			IDemo demo = (IDemo)mocks.StrictMock(typeof(IDemo));
			mocks.ReplayAll();
			try
			{
				demo.VoidNoArgs();
			}
			catch (Exception) { }
            Assert.Throws<ExpectationViolationException>(
                () => mocks.VerifyAll());
		}

		[Fact]
		public void DyamicMockAcceptUnexpectedCall()
		{
			MockRepository mocks = new MockRepository();
			IDemo demo = (IDemo)mocks.DynamicMock(typeof(IDemo));
			mocks.ReplayAll();
			demo.VoidNoArgs();
			mocks.VerifyAll();
		}

		[Fact]
		public void RepositoryThrowsWithConstructorArgsForMockInterface()
		{
			MockRepository mocks = new MockRepository();
			Assert.Throws<ArgumentException>(() =>
			{
				IDemo demo = (IDemo) mocks.StrictMock(typeof (IDemo), "Foo");
			});
		}

		[Fact]
		public void RepositoryThrowsWithConstructorArgsForMockDelegate()
		{
			MockRepository mocks = new MockRepository();
			Assert.Throws<ArgumentException>(() =>
			{
				EventHandler handler = (EventHandler) mocks.StrictMock(typeof (EventHandler), "Foo");
			});
		}

		[Fact]
		public void RepositoryThrowsWithWrongConstructorArgsForMockClass()
		{
			MockRepository mocks = new MockRepository();
			// There is no constructor on object that takes a string
			// parameter, so this should fail.
            try
            {
                object o = mocks.StrictMock(typeof(object), "Foo"); 
   
                Assert.False(true, "The above call should have failed");
            }
            catch(ArgumentException argEx)
            {
				Assert.Contains("Can not instantiate proxy of class: System.Object.", argEx.Message);
            }
		}

		[Fact]
		public void IsInReplayModeThrowsWhenPassedNull()
		{
			Assert.Throws<ArgumentNullException>(() => mocks.IsInReplayMode(null));
		}

		[Fact]
		public void IsInReplayModeThrowsWhenPassedNonMock()
		{
			Assert.Throws<ArgumentException>(() => mocks.IsInReplayMode(new object()));
		}

		[Fact]
		public void IsInReplayModeReturnsTrueWhenMockInReplay()
		{
			mocks.Replay(demo);

			Assert.True(mocks.IsInReplayMode(demo));
		}

		[Fact]
		public void IsInReplayModeReturnsFalseWhenMockInRecord()
		{
			Assert.False(mocks.IsInReplayMode(demo));
		}

        [Fact]
        public void GenerateMockForClassWithNoDefaultConstructor() 
        {
            Assert.NotNull(MockRepository.GenerateMock<ClassWithNonDefaultConstructor>(null, 0));            
        }

        [Fact]
        public void GenerateMockForClassWithDefaultConstructor() 
        {
            Assert.NotNull(MockRepository.GenerateMock<ClassWithDefaultConstructor>());
        }

        [Fact]
        public void GenerateMockForInterface() 
        {
            Assert.NotNull(MockRepository.GenerateMock<IDemo>());
        }

		[Fact]
		public void GenerateStrictMockWithRemoting()
		{
            IDemo mock = MockRepository.GenerateStrictMockWithRemoting<IDemo>();
			Assert.NotNull(mock);
#if DOTNET35
			Assert.True(mock.GetMockRepository().IsInReplayMode(mock));
#endif
		}

		[Fact]
		public void GenerateDynamicMockWithRemoting()
		{
            IDemo mock = MockRepository.GenerateDynamicMockWithRemoting<IDemo>();
			Assert.NotNull(mock);
#if DOTNET35
			Assert.True(mock.GetMockRepository().IsInReplayMode(mock));
#endif
        }

		public class ClassWithNonDefaultConstructor 
        {
            public ClassWithNonDefaultConstructor(string someString, int someInt) {}
        }
        public class ClassWithDefaultConstructor {}        

		#region Implementation

		private enum DemoEnum
		{
			Demo,
			NonDemo
		}

		#endregion
	}
}
