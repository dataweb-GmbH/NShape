using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NShapeTest
{

	/// <summary>
	/// Base class for NShape test cases
	/// </summary>
	public class NShapeTestBase
	{


		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get;
			set;
		}


		// You can use the following additional attributes as you write your tests:

		// Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static virtual void MyClassInitialize(TestContext testContext) { }


		// Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup() { }


		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public virtual void TestInitialize()
		{
			BeginTimer(TestContext?.TestName);
		}


		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public virtual void TestCleanup()
		{
			// Throws NotSupportedException in VisualStudio 2017
			EndTimer(TestContext?.TestName);
		}


		protected void BeginTimer(string timerName)
		{
			// Throws NotSupportedException in VisualStudio 2017
			//TestContext?.BeginTimer(timerName);
			Timers.Add(timerName, new Stopwatch());
			Timers[timerName].Start();
		}


		protected void EndTimer(string timerName)
		{
			// Throws NotSupportedException in VisualStudio 2017
			//TestContext?.EndTimer(timerName);
			Timers[timerName].Stop();
			TestContext.WriteLine("'{0}': {1}", timerName, Timers[timerName].Elapsed);
			Timers.Remove(timerName);
		}


		private Dictionary<string, Stopwatch> Timers { get; } = new Dictionary<string, Stopwatch>();

	}

}
