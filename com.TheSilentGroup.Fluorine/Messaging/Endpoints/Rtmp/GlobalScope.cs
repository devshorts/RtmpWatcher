using System;

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for GlobalScope.
	/// </summary>
	public class GlobalScope : Scope, IGlobalScope
	{
		protected IServer _server;

		public GlobalScope(IServer server)
		{
			_server = server;
		}

		#region IGlobalScope Members

		public void Register()
		{
			_server.RegisterGlobal(this);
			Init();
		}

		#endregion
	}
}
