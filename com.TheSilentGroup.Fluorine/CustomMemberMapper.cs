/*
	Customization by Samuel Neff (sam, atellis, com)
	Copyright (C) 2007 Atellis, Inc. (http://www.atellis.com)
	

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
using System.Configuration;
using com.TheSilentGroup.Fluorine.Activation;
using log4net;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// Provides for mapping custom object members when translating between Flash and .NET.
	/// </summary>
	public class CustomMemberMapper : ICustomMemberMapper
	{		
		private ICustomMemberMapper _realMapper;

		public static ICustomMemberMapper Instance = new CustomMemberMapper();

		private CustomMemberMapper()
		{
			string mapperType = ConfigurationManager.AppSettings["FluorineCustomMemberMapper"];
			if (mapperType != null && mapperType.Length != 0)
			{
				Exception ex = null;
				try
				{
					Type t = ObjectFactory.Locate(null, mapperType); 
					if (t != null && typeof(ICustomMemberMapper).IsAssignableFrom(t))
					{
						_realMapper = (ICustomMemberMapper)Activator.CreateInstance(t);
					}
				}
				catch(Exception ex2)
				{
					ex = ex2;
				}

				if (_realMapper == null)
				{
					ILog log =
						LogManager.GetLogger(typeof (CustomMemberMapper));
					if (log != null)
					{
						log.Error("Unable to load custom member mapper '" + mapperType + "'.", ex);
					}
				}
			}			
			if (_realMapper == null)
			{
				_realMapper = new NoMapper();
			}
		}

		public string ToFlash(string dotNetName)
		{
			return dotNetName == null || dotNetName.Length == 0
			       	?
			       dotNetName
			       	:
			       _realMapper.ToFlash(dotNetName);
		}

		public string ToDotNet(string flashName)
		{
			return flashName == null || flashName.Length == 0
			       	?
			       flashName
			       	:
			       _realMapper.ToDotNet(flashName);
		}

		#region NoMapper Implementation

		private class NoMapper : ICustomMemberMapper
		{
			public string ToFlash(string dotNetName)
			{
				return dotNetName;
			}

			public string ToDotNet(string flashName)
			{
				return flashName;
			}
		}

		#endregion
	}

	/// <summary>
	/// All custom member name mappers must implement this interface.
	/// </summary>
	public interface ICustomMemberMapper
	{
		string ToFlash(string dotNetName);
		string ToDotNet(string flashName);
	}

	/// <summary>
	/// Provides member mapping where .NET uses first-capital and Flash uses first-lower case
	/// member names.
	/// </summary>
	public class FirstCapMemberMapper : ICustomMemberMapper
	{
		public string ToFlash(string dotNetName)
		{
			if (dotNetName.Length == 1)
			{
				return dotNetName.ToLower();
			}
			else
			{
				return dotNetName.Substring(0, 1).ToLower() + dotNetName.Substring(1);
			}
		}

		public string ToDotNet(string flashName)
		{
			if (flashName.Length == 1)
			{
				return flashName.ToUpper();
			}
			else
			{
				return flashName.Substring(0, 1).ToUpper() + flashName.Substring(1);
			}
		}
	}
}
