#include "PacketOrderer.h"

PacketOrderer::PacketOrderer(){
	
}

PacketOrderer::~PacketOrderer(){
	
}

void PacketOrderer::AddPacket(const char * data, int length){
	int port = GetDestinationPort(data);

	auto aggregator = aggregatorsByPort.find(port);

	if(aggregator != aggregatorsByPort.end()){
		aggregator->second->Add(data, length);
	}
	else{
		RtmpPacketAggregator * newAggregator = new RtmpPacketAggregator(port);
		newAggregator->Add(data, length);
		aggregatorsByPort.insert(std::pair<int, RtmpPacketAggregator *>(port, newAggregator));
	}
}

RtmpPacket * PacketOrderer::PacketReady(){
	auto it = aggregatorsByPort.begin();
	RtmpPacket * packet;
	while(it != aggregatorsByPort.end()){
		packet = it->second->PacketReady();
		if(packet != NULL){
			return packet;
		}
		it++;
	}
	return NULL;
}

int PacketOrderer::GetDestinationPort(const char * data){
	TCPHEADER * tcpHeader = (TCPHEADER *)(data + sizeof(IPHEADER));
	return htons(tcpHeader->destination_port);
}

