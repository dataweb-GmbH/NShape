/******************************************************************************
  Copyright 2009-2021 dataweb GmbH
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
using System.Threading;


namespace Dataweb.NShape.Advanced {


	/// <ToBeCompleted></ToBeCompleted>
	public class Model : IEntity {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.Model" />
		/// </summary>
		public Model() { }
		
		
		#region IEntity Members

		/// <summary>
		/// Specifies the entity type name of <see cref="T:Dataweb.NShape.Advanced.Model" />.
		/// </summary>
		public static string EntityTypeName {
			get { return _entityTypeName; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.Model" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield break;
		}


		/// <summary>
		/// The <see cref="T:Dataweb.NShape.Advanced.IEntity" />.Id of this <see cref="T:Dataweb.NShape.Advanced.Model" />
		/// </summary>
		public object Id {
			get { return _id; }
		}


		/// <summary>
		/// See <see cref="T:Dataweb.NShape.Advanced.IEntity" />
		/// </summary>
		void IEntity.AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this._id != null) 
				throw new InvalidOperationException(string.Format("{0} has already an id.", GetType().Name));
			this._id = id;
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			// nothing to do
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			// nothing to do
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			// nothing to do
		}

		#endregion


		private const string _entityTypeName = "Core.Model";
		private object _id = null;
	}
	
	
	/// <summary>
	/// Defines a connection port for a model object.
	/// </summary>
	/// <status>reviewed</status>
	public struct TerminalId : IEquatable<TerminalId> {

		/// <summary>Specifies the invalid connection port.</summary>
		public static readonly TerminalId Invalid;

		/// <summary>Specifies a port for the model object as a whole.</summary>
		public static readonly TerminalId Generic;


		/// <summary>Converts a <see cref="T:Dataweb.NShape.Advanced.TerminalId" /> to a <see cref="T:System.Int32" />.</summary>
		public static implicit operator int(TerminalId tid) {
			return tid._id;
		}


		/// <summary>Converts a <see cref="T:System.Int32" /> to a <see cref="T:Dataweb.NShape.Advanced.TerminalId" />.</summary>
		public static implicit operator TerminalId(int value) {
			TerminalId result = Invalid;
			result._id = value;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool operator ==(TerminalId tid1, TerminalId tid2) {
			return tid1._id == tid2._id;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool operator !=(TerminalId tid1, TerminalId tid2) {
			return tid1._id != tid2._id;
		}


		/// <override></override>
		public override bool Equals(object obj) {
			return obj is TerminalId && (TerminalId)obj == this;
		}


		/// <override></override>
		public bool Equals(TerminalId other) {
			return other == this;
		}


		/// <override></override>
		public override int GetHashCode() {
			return _id.GetHashCode();
		}


		/// <override></override>
		public override string ToString() {
			return _id.ToString();
		}
		

		static TerminalId() {
			Invalid._id = int.MinValue;
			Generic._id = 0;
		}


		private int _id;
	}

}
