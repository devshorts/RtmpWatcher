/*
	Fluorine .NET Flash Remoting Gateway open source library 
	Copyright (C) 2005 Zoltan Csibi, zoltan@TheSilentGroup.com
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using com.TheSilentGroup.Fluorine;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class StatusASO : ASObject
	{
		public const string ERROR = "error";
		public const string STATUS = "status";
		public const string WARNING = "warning";

		public const string NC_CALL_FAILED = "NetConnection.Call.Failed";
		public const string NC_CALL_BADVERSION = "NetConnection.Call.BadVersion";
		public const string NC_CONNECT_APPSHUTDOWN = "NetConnection.Connect.AppShutdown";
		public const string NC_CONNECT_CLOSED = "NetConnection.Connect.Closed";
		public const string NC_CONNECT_FAILED = "NetConnection.Connect.Failed";
		public const string NC_CONNECT_REJECTED = "NetConnection.Connect.Rejected";
		public const string NC_CONNECT_SUCCESS = "NetConnection.Connect.Success";
		public const string NS_CLEAR_SUCCESS = "NetStream.Clear.Success";
		public const string NS_CLEAR_FAILED = "NetStream.Clear.Failed";
		public const string NS_PUBLISH_START = "NetStream.Publish.Start";
		public const string NS_PUBLISH_BADNAME = "NetStream.Publish.BadName";
		public const string NS_FAILED = "NetStream.Failed";
		public const string NS_UNPUBLISHED_SUCCESS = "NetStream.Unpublish.Success";
		public const string NS_RECORD_START = "NetStream.Record.Start";
		public const string NS_RECOED_NOACCESS = "NetStream.Record.NoAccess";
		public const string NS_RECORD_STOP = "NetStream.Record.Stop";
		public const string NS_RECORD_FAILED = "NetStream.Record.Failed";
		public const string NS_PLAY_INSUFFICIENT_BW = "NetStream.Play.InsufficientBW";
		public const string NS_PLAY_START = "NetStream.Play.Start";
		public const string NS_PLAY_STREAMNOTFOUND = "NetStream.Play.StreamNotFound";
		public const string NS_PLAY_STOP = "NetStream.Play.Stop";
		public const string NS_PLAY_FAILED = "NetStream.Play.Failed";
		public const string NS_PLAY_RESET = "NetStream.Play.Reset";
		public const string NS_PLAY_PUBLISHNOTIFY = "NetStream.Play.PublishNotify";
		public const string NS_PLAY_UNPUBLISHNOTIFY = "NetStream.Play.UnpublishNotify";
		public const string NS_SEEK_NOTIFY = "NetStream.Seek.Notify";
		public const string NS_PAUSE_NOTIFY = "NetStream.Pause.Notify";
		public const string NS_UNPAUSE_NOTIFY = "NetStream.Unpause.Notify";
		public const string NS_DATA_START = "NetStream.Data.Start";
		public const string APP_SCRIPT_ERROR = "Application.Script.Error";
		public const string APP_SCRIPT_WARNING = "Application.Script.Warning";
		public const string APP_RESOURCE_LOWMEMORY = "Application.Resource.LowMemory";
		public const string APP_SHUTDOWN = "Application.Shutdown";
		public const string APP_GC = "Application.GC";

		/// <summary>
		/// Initializes a new instance of the StatusASO class.
		/// </summary>
		public StatusASO()
		{
		}
		/// <summary>
		/// Initializes a new instance of the StatusASO class.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="level"></param>
		/// <param name="description"></param>
		/// <param name="application"></param>
		public StatusASO(string code, string level, string description, object application)
		{
			Add("code", code);
			Add("level", level);
			Add("description", description);
			Add("application", application);
		}

		public object Application
		{
			set{ this["application"] = value; }
		}

		public static StatusASO GetStatusObject(string statusCode)
		{
			switch(statusCode)
			{
				case NC_CALL_FAILED:
					return new StatusASO(NC_CALL_FAILED, ERROR, string.Empty, null);
				case NC_CALL_BADVERSION:
					return new StatusASO(NC_CALL_BADVERSION, ERROR, string.Empty, null);
				case NC_CONNECT_APPSHUTDOWN:
					return new StatusASO(NC_CONNECT_APPSHUTDOWN, ERROR, string.Empty, null);
				case NC_CONNECT_CLOSED:
					return new StatusASO(NC_CONNECT_CLOSED, ERROR, string.Empty, null);
				case NC_CONNECT_FAILED:
					return new StatusASO(NC_CONNECT_FAILED, ERROR, string.Empty, null);
				case NC_CONNECT_REJECTED:
					return new StatusASO(NC_CONNECT_REJECTED, ERROR, string.Empty, null);
				case NC_CONNECT_SUCCESS:
					return new StatusASO(NC_CONNECT_SUCCESS, STATUS, string.Empty, null);
				default:
					return new StatusASO(NC_CALL_BADVERSION, ERROR, string.Empty, null);
			}
		}
	}
}
