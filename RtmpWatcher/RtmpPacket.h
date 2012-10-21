#pragma once

#include "TcpPacket.h"
#include "TcpDefinitions.h"

class RtmpPacket : public TcpPacket{
public:
	enum RtmpDataTypes{
		Handshake,
		ChunkSize,
		Ping,
		ServerBandwidth,
		ClientBandwidth,
		Audio,
		Video,
		Notify,
		Invoke,
		AggregateMessage,
		Unknown
	};

	RtmpDataTypes rtmpPacketType;
};