#include "stdafx.h"

#include "RtmpPacketInterop.h"

using namespace System;
using namespace System::Runtime::InteropServices;

RtmpInterop::RtmpPacketInterop::RtmpPacketInterop(RtmpPacket * packet){
	_bytes = gcnew array<unsigned char>(packet->dataLength);

	Marshal::Copy(IntPtr(const_cast<void*>(static_cast<const void*>(packet->data))), _bytes, 0, packet->dataLength);

	_length = packet->dataLength;

	_sourceIP = gcnew String(packet->sourceIp);

	_destIp = gcnew String(packet->destIp);
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


