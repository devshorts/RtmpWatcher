#include "stdafx.h"

#include "RtmpPacketInterop.h"

using namespace System;
using namespace System::Runtime::InteropServices;

RtmpInterop::RtmpPacketInterop::RtmpPacketInterop(RtmpPacket * packet){
	bytes = gcnew array<unsigned char>(packet->dataLength);

	Marshal::Copy(IntPtr(const_cast<void*>(static_cast<const void*>(packet->data))), bytes, 0, packet->dataLength);

	length = packet->dataLength;

	sourceIP = gcnew String(packet->sourceIp);

	destIp = gcnew String(packet->destIp);
}