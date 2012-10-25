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
using System.Collections;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Data.Messages;

namespace com.TheSilentGroup.Fluorine.Data
{
	/// <summary>
	/// Summary description for DataDestination.
	/// </summary>
	internal class DataDestination : MessageDestination
	{
		private ILog _log;

		Hashtable		_sequenceIdToSequenceHash;
		Hashtable		_parametersToSequenceIdHash;
		Hashtable		_itemIdToSequenceIdMapHash;
		Hashtable		_itemIdToItemHash;

		public DataDestination(IService service, DestinationSettings destinationSettings) : base (service, destinationSettings)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(DataDestination));
			}
			catch{}
			_sequenceIdToSequenceHash = new Hashtable();
			_parametersToSequenceIdHash = new Hashtable(new ListHashCodeProvider(), new ListComparer());
			_itemIdToSequenceIdMapHash = new Hashtable();
			_itemIdToItemHash = new Hashtable();
		}

		public string[] GetIdentityKeys()
		{
			if( this.DestinationSettings.MetadataSettings != null )
			{
				ArrayList identity = this.DestinationSettings.MetadataSettings.Identity;
				return identity.ToArray(typeof(string)) as string[];
			}
			return new string[0];
		}

		private Sequence GetSequence(int sequenceId)
		{
			lock(_sequenceIdToSequenceHash)
			{
				return _sequenceIdToSequenceHash[sequenceId] as Sequence;
			}
		}

		private void RemoveSequence(int sequenceId)
		{
			lock(_sequenceIdToSequenceHash)
			{
				Sequence sequence = GetSequence(sequenceId);
				if( sequence != null )
				{
					for(int i = sequence.Count-1; i >= 0; i--)
					{
						Identity identity = sequence[i];
						RemoveIdentityFromSequence(sequence, identity, i);

					}
					if( sequence.Parameters != null )
							_parametersToSequenceIdHash.Remove(sequence.Parameters);

					_sequenceIdToSequenceHash.Remove(sequenceId);//clear entry
				}
			}
		}

		private int AddIdentityToSequence(Sequence sequence, Identity identity)
		{
			lock(_sequenceIdToSequenceHash)
			{
				int index = sequence.Add(identity);
				Hashtable sequenceIdMap = _itemIdToSequenceIdMapHash[identity] as Hashtable;
				if( sequenceIdMap == null )
				{
					sequenceIdMap = new Hashtable();
					_itemIdToSequenceIdMapHash[identity] = sequenceIdMap;
				}
				sequenceIdMap[sequence.Id] = sequence;
				return index;
			}
		}

		private void RemoveIdentityFromSequence(Sequence sequence, Identity identity, int position)
		{
			lock(_sequenceIdToSequenceHash)
			{
				Hashtable sequenceIdMap = _itemIdToSequenceIdMapHash[identity] as Hashtable;
				if( sequenceIdMap != null )
				{
					sequenceIdMap.Remove(sequence.Id);
					//Release the item if it does'n occur in any sequence
					if( sequenceIdMap.Count == 0 )
					{
						_itemIdToItemHash.Remove(identity);
						_itemIdToSequenceIdMapHash.Remove(identity);
					}
					sequence.RemoveAt(position);
				}
			}
		}

		private object GetItem(Identity identity)
		{
			lock(_sequenceIdToSequenceHash)
			{
				return _itemIdToItemHash[identity];
			}
		}


		public SequencedMessage CreateSequencedMessage(MessageContext messageContext, DataMessage dataMessage, IList result, IList parameters)
		{
			Sequence sequence = null;
			Identity[] identities = new Identity[result.Count];

			lock(_sequenceIdToSequenceHash)
			{
				for(int i = 0; i < identities.Length; i++)
				{
					if( result[i] != null )
					{
						Identity identity = Identity.GetIdentity(result[i], this);
						identities[i] = identity;
						if( ! _itemIdToItemHash.ContainsKey(identity) )
							_itemIdToItemHash.Add(identity, result[i]);
					}
				}
				//Lookup existing sequence
				if( parameters != null )
				{
					if( _parametersToSequenceIdHash.Contains(parameters) )
						sequence = _parametersToSequenceIdHash[parameters] as Sequence;
				}
				if( sequence == null )
				{
					sequence = new Sequence();
					sequence.Id = sequence.GetHashCode();
					if( parameters != null )
					{
						object[] parametersArray = new object[parameters.Count];
						parameters.CopyTo(parametersArray, 0);
						sequence.Parameters = parametersArray;
					}

					if( parameters != null )
						_parametersToSequenceIdHash[parameters] = sequence;

					for(int i = 0; i < identities.Length; i++)
					{
						Identity identity = identities[i];
						AddIdentityToSequence(sequence, identity);
					}

					_sequenceIdToSequenceHash[sequence.Id] = sequence;

					if( _log != null && _log.IsDebugEnabled )
						_log.Debug("Created sequence:" + sequence.Id + " client:" + dataMessage.clientId);

				}
				else
				{
					for(int i = 0; i < identities.Length; i++)
					{
						Identity identity = identities[i];
						Identity existingIdentity = null;
						if( i < sequence.Count )
							existingIdentity = sequence[i];
						if( !identity.Equals(existingIdentity) )
						{
							//Identity not found in sequence
							if( !sequence.Contains(identity) )
							{
								int position = AddIdentityToSequence(sequence, identity);

								UpdateCollectionMessage updateCollectionMessage = CreateUpdateCollectionMessage(dataMessage, sequence, identity, position, UpdateCollectionMessage.ServerUpdate);
								messageContext.PushOutgoingMessage( updateCollectionMessage);
							}
						}
					}
				}
				sequence.AddSubscriber(dataMessage.clientId as string);
			}

			if( dataMessage.headers != null && dataMessage.headers.ContainsKey("pageSize") )
			{
				return GetPagedMessage(dataMessage, sequence);
			}
			else
			{
				SequencedMessage sequencedMessage = new SequencedMessage();
				sequencedMessage.destination = dataMessage.destination;
				sequencedMessage.sequenceId = sequence.Id;
				sequencedMessage.sequenceSize = sequence.Size;
				//sequencedMessage.body = result;
				object[] body = new object[result.Count];
				result.CopyTo(body, 0);
				sequencedMessage.body = body;
				sequencedMessage.sequenceProxies = null;
				sequencedMessage.dataMessage = dataMessage;
				return sequencedMessage;
			}
		}

		public SequencedMessage GetPageItems(DataMessage dataMessage)
		{
			int sequenceId = (int)dataMessage.headers["sequenceId"];
			Sequence sequence = GetSequence(sequenceId);
			IList DSids = dataMessage.headers["DSids"] as IList;
			//ArrayList items = new ArrayList(DSids.Count);
			object[] items = new object[DSids.Count];
			for(int i = 0; i < DSids.Count; i++)
			{
				Identity identity = new Identity(DSids[i] as Hashtable);
				object item = GetItem(identity);
				//items.Add(item);
				items[i] = item;
			}
			
			SequencedMessage sequencedMessage = new SequencedMessage();
			lock( sequence )
			{
				sequencedMessage.destination = dataMessage.destination;
				sequencedMessage.sequenceId = sequence.Id;
				sequencedMessage.sequenceSize = sequence.Size;
				sequencedMessage.sequenceProxies = null;

				sequencedMessage.body = items;
			}
			return sequencedMessage;
		}

		public PagedMessage GetPagedMessage(DataMessage dataMessage, Sequence sequence)
		{
			int pageSize = (int)dataMessage.headers["pageSize"];
			int pageIndex = 0;
			if( dataMessage.headers.ContainsKey("pageIndex") )
				pageIndex = (int)dataMessage.headers["pageIndex"];
			int pageCount = (int)Math.Ceiling((double)sequence.Size / pageSize);
			int pageStart = pageIndex * pageSize;
			int pageEnd = Math.Min(pageStart + pageSize, sequence.Size);

			PagedMessage pagedMessage = new PagedMessage();
			pagedMessage.pageIndex = pageIndex;
			pagedMessage.pageCount = pageCount;
			pagedMessage.sequenceSize = sequence.Size;
			pagedMessage.sequenceId = sequence.Id;
			object[] pagedResult = new object[pageEnd-pageStart];
			for(int i = pageStart; i < pageEnd; i++)
			{
				Identity identity = sequence[i];
				//pagedResult.Add( _itemIdToItemHash[identity] );
				pagedResult[i-pageStart] = _itemIdToItemHash[identity];
			}
			pagedMessage.body = pagedResult;
			pagedMessage.destination = dataMessage.destination;
			pagedMessage.dataMessage = dataMessage;
			return pagedMessage;
		}

		public PagedMessage GetPage(DataMessage dataMessage)
		{
			int sequenceId = (int)dataMessage.headers["sequenceId"];
			Sequence sequence = GetSequence(sequenceId);
			return GetPagedMessage(dataMessage, sequence);
		}

		private UpdateCollectionMessage CreateUpdateCollectionMessage(DataMessage dataMessage, Sequence sequence, Identity identity, int position, int updateMode)
		{
			UpdateCollectionMessage updateCollectionMessage = new UpdateCollectionMessage();
			// The unique identifier for the collection that was updated. For a collection filled with the 
			// DataService.fill() method this contains an Array of the parameters specified.
			updateCollectionMessage.collectionId = sequence.Parameters;
			updateCollectionMessage.destination = dataMessage.destination;
			updateCollectionMessage.replace = false;
			updateCollectionMessage.updateMode = updateMode;
			updateCollectionMessage.messageId = "srv:" + Guid.NewGuid().ToString("D") + ":0";
			updateCollectionMessage.correlationId = dataMessage.correlationId;

			UpdateCollectionRange updateCollectionRange = new UpdateCollectionRange();
			// An Array of identity objects that represent which items were either deleted or inserted in the 
			// associated collection starting at the position indicated by the position property
			updateCollectionRange.identities = new object[1];
			//(updateCollectionRange.identities as IList).Add( identity );
			(updateCollectionRange.identities as object[])[0] = identity;
			updateCollectionRange.updateType = UpdateCollectionRange.InsertIntoCollection;
			updateCollectionRange.position = position;
					
			//ArrayList body = new ArrayList();
			//body.Add(updateCollectionRange);
			object[] body = new object[1]; body[0] = updateCollectionRange;
			updateCollectionMessage.body = body;
			return updateCollectionMessage;
		}

		public void ReleaseCollectionOperation(IApplicationContext applicationContext, DataMessage dataMessage)
		{
			lock(_sequenceIdToSequenceHash)
			{
				int sequenceId = (int)dataMessage.headers["sequenceId"];
				Sequence sequence = GetSequence(sequenceId);
				IList parameters = dataMessage.body as IList;
				sequence.RemoveSubscriber(dataMessage.clientId as string);

				if( _log != null && _log.IsDebugEnabled )
					_log.Debug("ReleaseCollection sequence:" + sequence.Id + " client:" + dataMessage.clientId);


				if( sequence.SubscriberCount == 0 )
				{
					if( _log != null && _log.IsDebugEnabled )
						_log.Debug("Removing sequence:" + sequence.Id + " client:" + dataMessage.clientId);
					RemoveSequence(sequenceId);
				}
			}
		}

		public void ReleaseItemOperation(DataMessage dataMessage)
		{
			int sequenceId = (int)dataMessage.headers["sequenceId"];
			Sequence sequence = GetSequence(sequenceId);
		}
	}
}
