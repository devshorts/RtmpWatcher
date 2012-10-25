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
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
// Import log4net classes.
using log4net;
using log4net.Config;
using com.TheSilentGroup.Fluorine.Gateway;

namespace com.TheSilentGroup.Fluorine.Util
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class WsdlHelper
	{
		/// <summary>
		/// Initializes a new instance of the WsdlHelper class.
		/// </summary>
		private WsdlHelper()
		{
		}

		#region Wsdl

		public static string GetWsdl(IApplicationContext applicationContext, string source) 
		{
			if(source.StartsWith("<?xml version") == true)
			{
				//wsdl string?
				return source;
			}
			else
			{
				if(source.StartsWith("http://") || source.StartsWith("https://")  )
				{
					return WsdlFromUrl(source);     // this is a url address
				}
				else    
					return WsdlFromFile(applicationContext, source);    // try to get from the file system
			}
		}

		public static string WsdlFromUrl(string url)
		{
			WebRequest webRequest = WebRequest.Create(url);
			WebResponse result = webRequest.GetResponse();
			Stream responseStream = result.GetResponseStream();
			Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
			StreamReader sr = new StreamReader( responseStream, encode );
			string wsdl = sr.ReadToEnd();
			return wsdl;
		}

		public static string WsdlFromFile(IApplicationContext applicationContext, string wsdlSource)
		{
			string fileFullPathName = applicationContext.MapPath(wsdlSource);
			FileInfo fi = new FileInfo(fileFullPathName);
			if(fi.Extension == ".wsdl")
			{
				FileStream fs = new FileStream(fileFullPathName, FileMode.Open, FileAccess.Read);
				StreamReader sr = new StreamReader(fs);
				char[] buffer = new char[(int)fs.Length];
				sr.ReadBlock(buffer, 0, (int)fs.Length);
				return new string(buffer);
			}
			return null;
		}

		public static Assembly GetAssemblyFromWsdl(IApplicationContext applicationContext, string strWsdl)
		{
			// Xml text reader
			StringReader wsdlStringReader = new StringReader(strWsdl);
			XmlTextReader tr = new XmlTextReader(wsdlStringReader);
			ServiceDescription sd = ServiceDescription.Read(tr);
			tr.Close();

			// WSDL service description importer 
			CodeNamespace codeNamespace;
			if( applicationContext != null )
				codeNamespace = new CodeNamespace(applicationContext.WsdlProxyNamespace); 
			else
				codeNamespace = new CodeNamespace("com.TheSilentGroup.Fluorine.Proxy"); 
			CodeCompileUnit codeCompileUnit = new CodeCompileUnit(); 
			codeCompileUnit.Namespaces.Add(codeNamespace); 
			codeNamespace = new CodeNamespace("com.TheSilentGroup.Fluorine"); 
			codeCompileUnit.Namespaces.Add(codeNamespace); 

			ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
			sdi.AddServiceDescription(sd, null, null);
			sdi.ProtocolName = "Soap";
			sdi.Import(codeNamespace, codeCompileUnit);

			//http://support.microsoft.com/default.aspx?scid=kb;en-us;326790
			//http://pluralsight.com/wiki/default.aspx/Craig.RebuildingWsdlExe
			if( applicationContext != null && !applicationContext.WsdlGenerateProxyClasses )
			{
			
				//Strip any code that isn't the proxy class itself
				foreach(CodeNamespace cns in codeCompileUnit.Namespaces)
				{
					// Remove anything that isn't the proxy itself
					ArrayList typesToRemove = new ArrayList(); 
					foreach(CodeTypeDeclaration codeType in cns.Types)
					{
						bool webDerived = false; 
						foreach(CodeTypeReference baseType in codeType.BaseTypes)
						{
							if(baseType.BaseType == "System.Web.Services.Protocols.SoapHttpClientProtocol")
							{
								webDerived = true;
								break; 
							}
						}
						if (!webDerived)
							typesToRemove.Add(codeType); 
						else
						{
							CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration("RemotingService");
							codeType.CustomAttributes.Add(codeAttributeDeclaration);
						}
					}

					foreach (CodeTypeDeclaration codeType in typesToRemove)
					{
						codeNamespace.Types.Remove(codeType); 
					}
				}

				if (applicationContext.ImportNamespaces != null)
				{
					for (int i = 0; i < applicationContext.ImportNamespaces.Count; i++)
					{
						string key = applicationContext.ImportNamespaces.GetKey(i);
						codeNamespace.Imports.Add(new CodeNamespaceImport(key));
					}
				}
			}

			// source code generation
			CSharpCodeProvider cscp = new CSharpCodeProvider();
			ICodeGenerator icg = cscp.CreateGenerator();
			StringBuilder srcStringBuilder = new StringBuilder();
			StringWriter sw = new StringWriter(srcStringBuilder);
			//icg.GenerateCodeFromNamespace(codeNamespace, sw, null);
			icg.GenerateCodeFromCompileUnit(codeCompileUnit, sw, null);
			string srcWSProxy = srcStringBuilder.ToString();
			sw.Close();

			// assembly compilation.
			CompilerParameters cp = new CompilerParameters();
			cp.ReferencedAssemblies.Add("System.dll");
			cp.ReferencedAssemblies.Add("System.Data.dll");
			cp.ReferencedAssemblies.Add("System.Xml.dll");
			cp.ReferencedAssemblies.Add("System.Web.Services.dll");

			foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.GlobalAssemblyCache)
				{
					//Only System namespace
					if( assembly.GetName().Name.StartsWith("System") )
					{
						if( !cp.ReferencedAssemblies.Contains(assembly.GetName().Name + ".dll") )
							//cp.ReferencedAssemblies.Add(assembly.GetName().Name + ".dll");
							cp.ReferencedAssemblies.Add(assembly.Location);
					}
				}
				else
				{
					if (assembly.GetName().Name.StartsWith("mscorlib"))
						continue;
					//if( assembly.Location.ToLower().StartsWith(System.Web.HttpRuntime.CodegenDir.ToLower()) )
					//	continue;

					try
					{
						if (assembly.Location != null && assembly.Location != string.Empty)
							cp.ReferencedAssemblies.Add(assembly.Location);
					}
					catch (NotSupportedException)
					{
						//NET2
					}
				}
			}

			cp.GenerateExecutable = false;
			//http://support.microsoft.com/kb/815251
			//http://support.microsoft.com/kb/872800
			cp.GenerateInMemory = false;//true; 
			cp.IncludeDebugInformation = false; 
			ICodeCompiler icc = cscp.CreateCompiler();
			CompilerResults cr = icc.CompileAssemblyFromSource(cp, srcWSProxy);
			if(cr.Errors.Count > 0)
			{
				ILog log = null;
				try
				{
					log = LogManager.GetLogger(typeof(WsdlHelper));
				}
				catch{}
				StringBuilder sbMessage = new StringBuilder();
				sbMessage.Append(string.Format("Build failed: {0} errors", cr.Errors.Count));
				if( log != null && log.IsErrorEnabled )
					log.Error("WSDL service assembly compilation failed");
				for(int i = 0; i < cr.Errors.Count; i++)
				{
					if( log != null && log.IsErrorEnabled )
						log.Error(">> " + cr.Errors[i].ErrorText);
					sbMessage.Append("\n");
					sbMessage.Append(cr.Errors[i].ErrorText);
				}
				throw new ApplicationException(sbMessage.ToString()); 
			}

			return cr.CompiledAssembly;
		}

		#endregion Wsdl

	}
}
