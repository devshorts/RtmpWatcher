#include "stdafx.h"

#include "RtmpPacketInterop.h"
#include "RtmpPacket.h"

using namespace System;
using namespace System::Runtime::InteropServices;

RtmpInterop::RtmpPacketInterop::RtmpPacketInterop(RtmpPacket * packet){
	_bytes = gcnew array<unsigned char>(packet->dataLength);

	Marshal::Copy(IntPtr(const_cast<void*>(static_cast<const void*>(packet->data))), _bytes, 0, packet->dataLength);

	_length = packet->dataLength;

	_sourceIP = gcnew String(packet->sourceIp);

	_destIp = gcnew String(packet->destIp);

	switch(packet->rtmpPacketType){
		case RtmpPacket::RtmpDataTypes::Handshake: _packetType = RtmpPacketTypeManaged::RtmpPacketType::Handshake; break;
		case RtmpPacket::RtmpDataTypes::ChunkSize: _packetType = RtmpPacketTypeManaged::RtmpPacketType::ChunkSize; break;
		case RtmpPacket::RtmpDataTypes::Ping: _packetType = RtmpPacketTypeManaged::RtmpPacketType::Ping; break;
		case RtmpPacket::RtmpDataTypes::ServerBandwidth: _packetType = RtmpPacketTypeManaged::RtmpPacketType::ServerBandwidth; break;
		case RtmpPacket::RtmpDataTypes::ClientBandwidth: _packetType = RtmpPacketTypeManaged::RtmpPacketType::ClientBandwidth; break;
		case RtmpPacket::RtmpDataTypes::Audio: _packetType = RtmpPacketTypeManaged::RtmpPacketType::Audio; break;
		case RtmpPacket::RtmpDataTypes::Video: _packetType = RtmpPacketTypeManaged::RtmpPacketType::Video; break;
		case RtmpPacket::RtmpDataTypes::Notify: _packetType = RtmpPacketTypeManaged::RtmpPacketType::Notify; break;
		case RtmpPacket::RtmpDataTypes::Invoke: _packetType = RtmpPacketTypeManaged::RtmpPacketType::Invoke; break;
		case RtmpPacket::RtmpDataTypes::AggregateMessage: _packetType = RtmpPacketTypeManaged::RtmpPacketType::AggregateMessage; break;
	}
}

RtmpInterop::RtmpPacketTypeManaged::RtmpPacketType RtmpInterop::RtmpPacketInterop::GetRtmpPacketType(){
	return _packetType;
}

array<unsigned char>^ RtmpInterop::RtmpPacketInterop::GetBytes(){
	return _bytes;
}

String ^ RtmpInterop::RtmpPacketInterop::GetSourceIP(){
	return _sourceIP;
}

String ^ RtmpInterop::RtmpPacketInterop::GetDestIP(){
	return _destIp;
}

int RtmpInterop::RtmpPacketInterop::GetLength(){
	return _length;
}


