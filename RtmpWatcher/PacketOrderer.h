#pragma once

#include "RtmpPacketAggregator.h"
#include "RtmpPacket.h"

class PacketOrderer
{
public:
	PacketOrderer();
	~PacketOrderer();
	void AddPacket(char * data, int length);
	RtmpPacket * PacketReady();
private:
	RtmpPacketAggregator aggregator;
};

