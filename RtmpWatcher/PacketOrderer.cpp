#include "PacketOrderer.h"

PacketOrderer::PacketOrderer(){
	
}

PacketOrderer::~PacketOrderer(){
	
}

void PacketOrderer::AddPacket(char * data, int length){
	aggregator.Add(data, length);
}

RtmpPacket * PacketOrderer::PacketReady(){
	return aggregator.PacketReady();
}
