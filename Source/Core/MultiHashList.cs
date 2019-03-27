/******************************************************************************
  Copyright 2009-2017 dataweb GmbH
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
			Element newElement = new Element(key, value);
			if (_list[(int)(key % _listCapacity)] == null)
				_list[(int)(key % _listCapacity)] = newElement;
			else {
				Element e;
#if DEBUG_DIAGNOSTICS
				int cnt = 0;
				for (e = _list[(int)(key % _list.Capacity)]; !e.item.Equals(value) && e.next != null; e = e.next) 
					++cnt;
				if (cnt > _maxListLen) _maxListLen = cnt;
				else if (cnt > 0 && cnt < _minListLen) _minListLen = cnt;
#else
				for (e = _list[(int)(key % _listCapacity)]; !(e.item.Equals(value) && e.key == key) && e.next != null; e = e.next) ;
#endif
				// Do not insert the same shape with the same key a second time.
				// TODO 2: Optimize the second comparison.
				if (!(e.item.Equals(value) && e.key == key)) e.next = newElement;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool Remove(uint key, T value) {
			if (_list[(int)(key % _listCapacity)] == null) return false;
			Element e;
			if (_list[(int)(key % _listCapacity)].item.Equals(value)) {
				_list[(int)(key % _listCapacity)] = _list[(int)(key % _list.Capacity)].next;
				return true;
			} else {
				for (e = _list[(int)(key % _list.Capacity)]; 
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
				if (_list[(int)(key % _listCapacity)] == null) yield break;
				for (Element e = _list[(int)(key % _listCapacity)]; e != null; e = e.next)
					if (e.key == key) yield return e.item;
			}
		}


#if DEBUG_DIAGNOSTICS
		
		/// <summary>
		/// Returns the number of entries in the longest list.
		/// </summary>
		public int MaxListLength {
			get { return _maxListLen; }
		}


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
#if DEBUG_DIAGNOSTICS
		private int _maxListLen = 0;
		private int _minListLen = 0;
#endif
	}

}