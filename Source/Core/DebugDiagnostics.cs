using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dataweb.NShape {

	/// <summary>Diagnostics helper class.</summary>
	public static class DebugDiagnostics {

		/// <summary>Name of the define to activate the assertions.</summary>
		public const string DebugDiagnosticsDefine = "DEBUG_DIAGNOSTICS";


		/// <summary>Checks if the assertion condition is fulfilled, throws an exception if not.</summary>
		/// <param name="condition"></param>
		[Conditional(DebugDiagnosticsDefine)]
		public static void Assert(bool condition) {
			Assert(condition, null);
		}


		/// <summary>Checks if the assertion condition is fulfilled, throws an exception if not.</summary>
		[Conditional(DebugDiagnosticsDefine)]
		public static void Assert(bool condition, string message) {
			if (condition == false) {
				if (string.IsNullOrEmpty(message))
					throw new NShapeInternalException("Assertion Failure.");
				else
					throw new NShapeInternalException(string.Format("Assertion Failure: {0}", message));
			}
		}


		/// <summary>Invokes the provided action.</summary>
		[Conditional(DebugDiagnosticsDefine)]
		public static void Invoke(Action action) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			action();
		}

	}

}
