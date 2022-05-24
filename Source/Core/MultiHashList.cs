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

using System.Collections.Generic;


namespace Dataweb.Utilities {

	/// <summary>
	/// Implements a list, whose index is a hash value and which can have multiple items
	/// per index value.
	/// </summary>
	public class MultiHashList<T>  {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.Utilities.MultiHashList`1" />.
		/// </summary>
		/// <param name="capacity"></param>
		public MultiHashList(int capacity) {
			_listCapacity = capacity / _order;
			_list = new List<Element>(_listCapacity);
			for (int i = 0; i < _listCapacity; ++i) _list.Add(null);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Add(uint key, T value) {
#if DEBUG_DIAGNOSTICS
			_ListLength = 0;
#endif
			Element newElement = new Element(key, value);
			int idx = (int)(key % _listCapacity);
			if (_list[idx] == null)
				_list[idx] = newElement;
			else {
				Element e;
				for (e = _list[idx]; !(e.item.Equals(value) && e.key == key) && e.next != null; e = e.next)
#if DEBUG_DIAGNOSTICS
				{ _ListLength += 1; }
				if (_ListLength > 0) {
					if (_ListLength > MaxListLength) 
						MaxListLength = _ListLength;
					else if (_ListLength > 0 && _ListLength < MinListLength)
						MinListLength = _ListLength;
				}
#else
				{ /* Empty for loop body */ }
#endif
				//
				// Do not insert the same shape with the same key a second time.
				// TODO 2: Optimize the second comparison.
				if (!(e.item.Equals(value) && e.key == key)) e.next = newElement;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool Remove(uint key, T value) {
			int idx = (int)(key % _listCapacity);
			if (_list[idx] == null) return false;
			Element e;
			if (_list[idx].item.Equals(value)) {
				_list[idx] = _list[idx].next;
				return true;
			} else {
				for (e = _list[idx]; 
					e.next != null && (e.next.key != key || !e.next.item.Equals(value)); 
					e = e.next) ;
				// Either e.next is null or the one we are searching for
				if (e.next == null) return false;
				e.next = e.next.next;
				return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Clear() {
			// Clear list by overwriting all items with null
			for (int i = _list.Count - 1; i >= 0; --i) 
				_list[i] = null;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<T> this[uint key] {
			get {
				int idx = (int)(key % _listCapacity);
				if (_list[idx] == null) yield break;
				for (Element e = _list[idx]; e != null; e = e.next)
					if (e.key == key) yield return e.item;
			}
		}


#if DEBUG_DIAGNOSTICS

		/// <summary>Returns the number of entries in the longest list.</summary>
		public int MaxListLength { get; private set; } = 0;


		/// <summary>Returns the number of entries in the longest list.</summary>
		public int MinListLength { get; private set; } = int.MaxValue;


		/// <summary>
		/// Returns the nuber of lists that are not empty.
		/// </summary>
		public int ListCount {
			get {
				int cnt = 0;
				for (int i = _list.Count - 1; i >= 0; --i ) {
					if (_list[i] != null) ++cnt;
				}
				return cnt;
			}
		}

		private int _ListLength;

#endif


		private class Element {
			public Element(uint key, T item) {
				this.key = key;
				this.item = item;
			}
			public uint key;
			public T item;
			public Element next;
		}


		private const int _order = 3;
		private int _listCapacity;
		private List<Element> _list;
	}

}
