/******************************************************************************
  Copyright 2009-2022 dataweb GmbH
  This file is part of the NShape framework.
  NShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  NShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Resources;

namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Helper class used for converting a single instance into an IEnumerable&lt;T&gt;.
	/// </summary>
	public struct SingleInstanceEnumerator<T> : IEnumerable, IEnumerable<T>, IEnumerator<T>, IDisposable {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.SingleInstanceEnumerator`1" />.
		/// </summary>
		/// <returns></returns>
		public static SingleInstanceEnumerator<T> Create(T instance) {
			SingleInstanceEnumerator<T> result = SingleInstanceEnumerator<T>.Empty;
			result._instanceReturned = false;
			result._instance = instance;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static readonly SingleInstanceEnumerator<T> Empty;


		#region IEnumerable<T> Members

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerator<T> GetEnumerator() {
			return this;
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return this;
		}

		#endregion


		#region IEnumerator<T> Members

		/// <ToBeCompleted></ToBeCompleted>
		public T Current {
			get {
				if (_instanceReturned) throw new InvalidOperationException();
				_instanceReturned = true;
				return _instance;
			}
		}

		#endregion


		#region IEnumerator Members

		object IEnumerator.Current {
			get { return Current; }
		}

		/// <ToBeCompleted></ToBeCompleted>
		public bool MoveNext() {
			return !_instanceReturned;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public void Reset() {
			_instanceReturned = false;
		}

		#endregion


		#region IDisposable Members

		/// <ToBeCompleted></ToBeCompleted>
		public void Dispose() {
			// nothing to do
		}

		#endregion


		private bool _instanceReturned;
		private T _instance;
	}


	/// <summary>
	/// Helper class used for creating or comparing empty collections
	/// </summary>
	public struct EmptyEnumerator<T> : IEnumerable, IEnumerable<T>, IEnumerator<T>, IDisposable {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.EmptyEnumerator`1" />.
		/// </summary>
		/// <returns></returns>
		public static EmptyEnumerator<T> Create() {
			EmptyEnumerator<T> result = EmptyEnumerator<T>.Empty;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static readonly EmptyEnumerator<T> Empty;


		#region IEnumerable<T> Members

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerator<T> GetEnumerator() {
			return this;
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return this;
		}

		#endregion


		#region IEnumerator<T> Members

		/// <ToBeCompleted></ToBeCompleted>
		public T Current {
			get { return default(T); }
		}

		#endregion


		#region IEnumerator Members

		object IEnumerator.Current {
			get { return default(T); }
		}

		/// <ToBeCompleted></ToBeCompleted>
		public bool MoveNext() {
			return false; ;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public void Reset() {
			// nothing to do
		}

		#endregion


		#region IDisposable Members

		/// <ToBeCompleted></ToBeCompleted>
		public void Dispose() {
			// nothing to do
		}

		#endregion

	}


	/// <summary>
	/// Helper class used for converting collections of <see cref="T:System.Object" /> to collections of &lt;T&gt;.
	/// </summary>
	public struct ConvertEnumerator<T> : IEnumerable, IEnumerable<T>, IEnumerator<T>, IDisposable {

		/// <ToBeCompleted></ToBeCompleted>
		public static ConvertEnumerator<T> Create(IEnumerable enumeration) {
			return Create(enumeration.GetEnumerator());
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static ConvertEnumerator<T> Create(IEnumerator enumerator) {
			ConvertEnumerator<T> result = ConvertEnumerator<T>.Empty;
			result._enumerator = enumerator;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static readonly ConvertEnumerator<T> Empty;


		#region IEnumerable<T> Members

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerator<T> GetEnumerator() {
			return this;
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return this;
		}

		#endregion


		#region IEnumerator<T> Members

		/// <ToBeCompleted></ToBeCompleted>
		public T Current {
			get {
				if (_enumerator.Current is T)
					return (T)_enumerator.Current;
				else return default(T);
			}
		}

		#endregion


		#region IEnumerator Members

		object IEnumerator.Current {
			get {
				if (_enumerator.Current is T)
					return (T)_enumerator.Current;
				else return default(T);
			}
		}

		/// <ToBeCompleted></ToBeCompleted>
		public bool MoveNext() {
			bool result;
			do {
				result = _enumerator.MoveNext();
				if (!result) break;
			} while (!(_enumerator.Current is T));
			return result;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public void Reset() {
			_enumerator.Reset();
		}

		#endregion


		#region IDisposable Members

		/// <ToBeCompleted></ToBeCompleted>
		public void Dispose() {
			// nothing to do
		}

		#endregion


		private IEnumerator _enumerator;
	}


	///// <summary>
	///// Helper class used for counting the number of instances in an enumeration.
	///// </summary>
	//public static class Counter {

	//    /// <summary>
	//    /// Counts the number of instances in an enumeration.
	//    /// </summary>
	//    public static int GetCount<T>(IEnumerable<T> instances) {
	//        if (instances is ICollection)
	//            return ((ICollection)instances).Count;
	//        else if (instances is ICollection<T>)
	//            return ((ICollection<T>)instances).Count;
	//        else {
	//            int result = 0;
	//            IEnumerator<T> enumerator = instances.GetEnumerator();
	//            while (enumerator.MoveNext()) ++result;
	//            return result;
	//        }
	//    }

	//}


	/// <summary>
	/// Provides useful methods for efficiently iterating over objects and collections.
	/// </summary>
	public static class EnumerationHelper {

		/// <summary>
		/// Checks whether item is contained in the collection.
		/// </summary>
		public static Boolean Contains<T>(IEnumerable<T> items, T item)
		{
			foreach (T currItem in items)
				if (currItem.Equals(item))
					return true;
			return false;
		}


		/// <summary>
		/// Iterates over the given objects without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(T item) {
			if (item != null) yield return item;
		}


		/// <summary>
		/// Iterates over the given objects without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(T item1, T item2) {
			if (item1 != null) yield return item1;
			if (item2 != null) yield return item2;
		}


		/// <summary>
		/// Iterates over the given objects without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(T item1, T item2, T item3) {
			if (item1 != null) yield return item1;
			if (item2 != null) yield return item2;
			if (item3 != null) yield return item3;
		}


		/// <summary>
		/// Iterates over the given objects without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(params T[] items) {
			foreach (T item in items)
				if (item != null) yield return item;
		}


		/// <summary>
		/// Iterates over the given objects without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(T item, IEnumerable<T> items) {
			if (item != null) yield return item;
			foreach (T itm in items)
				yield return itm;
		}


		/// <summary>
		/// Iterates over the given objects without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(T item1, T item2, IEnumerable<T> items) {
			if (item1 != null) yield return item1;
			if (item2 != null) yield return item2;
			foreach (T itm in items)
				yield return itm;
		}


		/// <summary>
		/// Iterates over the given objects without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(T item1, T item2, T item3, IEnumerable<T> items) {
			if (item1 != null) yield return item1;
			if (item2 != null) yield return item2;
			if (item3 != null) yield return item3;
			foreach (T itm in items)
				yield return itm;
		}


		/// <summary>
		/// Enumerates over the given enumerables without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(IEnumerable<T> items1, IEnumerable<T> items2) {
			foreach (IEnumerable<T> items in Enumerate(items1, items2))
				foreach (T item in items)
					yield return item;
		}


		/// <summary>
		/// Enumerates over the given enumerables without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(IEnumerable<T> items1, IEnumerable<T> items2, IEnumerable<T> items3) {
			foreach (IEnumerable<T> items in Enumerate(items1, items2, items3))
				foreach (T item in items)
					yield return item;
		}


		/// <summary>
		/// Enumerates over the given enumerables without creating new collections.
		/// </summary>
		public static IEnumerable<T> Enumerate<T>(params IEnumerable<T>[] itemCollections) {
			if (itemCollections == null) throw new ArgumentNullException(nameof(itemCollections));
			foreach (IEnumerable<T> items in itemCollections)
				foreach (T item in items)
					yield return item;
		}


		/// <summary>
		/// Returns an empty <see cref="T: System.Collections.Generic.IEnumerable`T" />.
		/// </summary>
		public static IEnumerable<T> Empty<T>() {
			yield break;
		}


		/// <summary>
		/// Returns the first item of the collection of the default value of T.
		/// </summary>
		public static T First<T>(IEnumerable<T> items)
		{
			foreach (T item in items)
				return item;
			return default(T);
		}


		/// <summary>
		/// Returns true if the given enumeration has no items.
		/// </summary>
		public static Boolean IsEmpty<T>(IEnumerable<T> items) {
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (items is ICollection<T>)
				return ((ICollection<T>)items).Count == 0;
			else 
				return !items.GetEnumerator().MoveNext();
		}


		/// <summary>
		/// Returns the number of items in the collection.
		/// </summary>
		public static int Count<T>(IEnumerable<T> items)
		{
			if (items is ICollection collection)
				return collection.Count;
			else if (items is ICollection<T> collectionT)
				return collectionT.Count;
			else if (items is IList list)
				return list.Count;
			else if (items is IList<T> listT)
				return listT.Count;
			else if (items is T[] array)
				return array.Length;
			else {
				int result = 0;
				foreach (T item in items)
					++result;
				return result;
			}
		}

	}


	/// <summary>Class for generating hash codes based on fields.</summary>
	public static class HashCodeGenerator {

		/// <summary>Calculates and returns a hash code for the given fields.</summary>
		public static int CalculateHashCode<T1, T2>(T1 field1, T2 field2) {
			int result = SeedNumber;
			unchecked {
				if (field1 != null) result = result * FieldMultiplicator + field1.GetHashCode();
				if (field2 != null) result = result * FieldMultiplicator + field2.GetHashCode();
			}
			return result;
		}


		/// <summary>Calculates and returns a hash code for the given fields.</summary>
		public static int CalculateHashCode<T1, T2, T3>(T1 field1, T2 field2, T3 field3) {
			int result = SeedNumber;
			unchecked {
				if (field1 != null) result = result * FieldMultiplicator + field1.GetHashCode();
				if (field2 != null) result = result * FieldMultiplicator + field2.GetHashCode();
				if (field3 != null) result = result * FieldMultiplicator + field3.GetHashCode();
			}
			return result;
		}


		/// <summary>Calculates and returns a hash code for the given fields.</summary>
		public static int CalculateHashCode<T1, T2, T3, T4>(T1 field1, T2 field2, T3 field3, T4 field4) {
			int result = SeedNumber;
			unchecked {
				if (field1 != null) result = result * FieldMultiplicator + field1.GetHashCode();
				if (field2 != null) result = result * FieldMultiplicator + field2.GetHashCode();
				if (field3 != null) result = result * FieldMultiplicator + field3.GetHashCode();
				if (field4 != null) result = result * FieldMultiplicator + field4.GetHashCode();
			}
			return result;
		}


		/// <summary>Calculates and returns a hash code for the given fields.</summary>
		public static int CalculateHashCode<T1, T2, T3, T4, T5>(T1 field1, T2 field2, T3 field3, T4 field4, T5 field5) {
			int result = SeedNumber;
			unchecked {
				if (field1 != null) result = result * FieldMultiplicator + field1.GetHashCode();
				if (field2 != null) result = result * FieldMultiplicator + field2.GetHashCode();
				if (field3 != null) result = result * FieldMultiplicator + field3.GetHashCode();
				if (field4 != null) result = result * FieldMultiplicator + field4.GetHashCode();
				if (field5 != null) result = result * FieldMultiplicator + field5.GetHashCode();
			}
			return result;
		}


		// Prime numbers for calculating the hash code. 
		// We use prime numbers 17 and 23, could also be other prime numbers
		private const int SeedNumber = 17;
		private const int FieldMultiplicator = 23;

	}

}