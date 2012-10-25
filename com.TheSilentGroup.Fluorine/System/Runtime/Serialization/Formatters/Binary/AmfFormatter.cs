using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Runtime.Serialization.Formatters.Binary
{
	//http://www.winterdom.com/weblog/CommentView,guid,52.aspx
	//http://ngrid.sourceforge.net/wp/?p=14

	//testing
	//System.Runtime.Serialization.Formatters.Binary.AmfFormatter amfFormatter = new System.Runtime.Serialization.Formatters.Binary.AmfFormatter();
	//object obj = amfFormatter.Deserialize(requestStream);

	/// <summary>
	/// Summary description for AmfFormatter.
	/// </summary>
	public class AmfFormatter : IFormatter
	{
		public AmfFormatter()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region IFormatter Members

		public void Serialize(System.IO.Stream serializationStream, object graph)
		{
			// TODO:  Add AmfFormatter.Serialize implementation
		}

		public SerializationBinder Binder
		{
			get
			{
				// TODO:  Add AmfFormatter.Binder getter implementation
				return null;
			}
			set
			{
				// TODO:  Add AmfFormatter.Binder setter implementation
			}
		}

		public StreamingContext Context
		{
			get
			{
				// TODO:  Add AmfFormatter.Context getter implementation
				return new StreamingContext ();
			}
			set
			{
				// TODO:  Add AmfFormatter.Context setter implementation
			}
		}

		public ISurrogateSelector SurrogateSelector
		{
			get
			{
				// TODO:  Add AmfFormatter.SurrogateSelector getter implementation
				return null;
			}
			set
			{
				// TODO:  Add AmfFormatter.SurrogateSelector setter implementation
			}
		}

		public object Deserialize(System.IO.Stream serializationStream)
		{
			// TODO:  Add AmfFormatter.Deserialize implementation
			return null;
		}

		#endregion

	}
}
