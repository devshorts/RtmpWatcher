using System;
using System.Collections;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api
{
	/// <summary>
	/// Summary description for BasicScopeEnumerator.
	/// </summary>
	public sealed class BasicScopeEnumerator : IEnumerator
	{
		private int _index;
		private IList _enumerable;
		private IBasicScope _currentElement;


		internal BasicScopeEnumerator(IList enumerable)
		{
			_index = -1;
			_enumerable = enumerable;
		}

		#region IEnumerator Members

		public void Reset()
		{
			_currentElement = null;
			_index = -1;
		}

		public object Current
		{
			get
			{
				if(_index == -1)
					throw new InvalidOperationException("Enum not started.");
				if(_index >= _enumerable.Count)
					throw new InvalidOperationException("Enumeration ended.");
				return _currentElement;
			}
		}

		public bool MoveNext()
		{
			if(_index < _enumerable.Count - 1)
			{
				_index++;
				_currentElement = _enumerable[_index] as IBasicScope;
				return true;
			}
			_index = _enumerable.Count;
			return false;
		}

		#endregion
	}
}
