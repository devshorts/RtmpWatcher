#include "PacketOrderer.h"

PacketOrderer::PacketOrderer(){
	
}

PacketOrderer::~PacketOrderer(){
	
}

void PacketOrderer::AddPacket(const char * data, int length){
	aggregator.Add(data, length);
}

RtmpPacket * PacketOrderer::PacketReady(){
	return aggregator.PacketReady();
}
