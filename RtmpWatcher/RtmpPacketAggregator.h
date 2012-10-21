#pragma once

#include "RtmpPacket.h"

class RtmpPacketAggregator
{
public:
	RtmpPacketAggregator(void);
	void Add(char * data, int bytesTotal);
	RtmpPacket * PacketReady();

private:
	RtmpPacket::RtmpDataTypes GetRtmpPacketType(unsigned char * data);
	RtmpPacket::RtmpDataTypes packetType;
	int totalExpected;
	int totalFound;
	
	bool foundStart;
	char * dataCopy;
};

