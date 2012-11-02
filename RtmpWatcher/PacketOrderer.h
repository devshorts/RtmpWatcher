#pragma once

#include "RtmpPacketAggregator.h"
#include "RtmpPacket.h"

#include <map>
#include <vector>

class PacketOrderer
{
public:
	PacketOrderer();
	~PacketOrderer();
	void AddPacket(const char * ipFragment, int length);
	RtmpPacket * PacketReady();
private:
	void TranslateIP(unsigned int ipInt, char *ipString);

	std::map<int, RtmpPacketAggregator *> aggregatorsByPort;

	int GetDestinationPort(const char * ipFragment);
};

