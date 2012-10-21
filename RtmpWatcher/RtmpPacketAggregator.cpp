#include "RtmpPacketAggregator.h"
#include "TcpDefinitions.h"

RtmpPacketAggregator::RtmpPacketAggregator(void)
{
	dataCopy = NULL;
	foundStart = false;
	totalExpected = 0;
}

void RtmpPacketAggregator::Add(char * data, int bytesTotal){
	char * payload = data + sizeof(IPHEADER) + sizeof(TCPHEADER);

	int sizeOfPayload = bytesTotal - sizeof(IPHEADER) - sizeof(TCPHEADER);

	if(sizeOfPayload <= 0){
		return;
	}

	RtmpPacket::RtmpDataTypes payloadType = GetRtmpPacketType((unsigned char *)payload);

	if(payloadType != RtmpPacket::RtmpDataTypes::Unknown && !foundStart){
		// start of packet
		foundStart = true;

		totalFound = 0;

		unsigned char totalExpected_byte0 = *(payload + 4);
		unsigned char totalExpected_byte1 = *(payload + 5);
		unsigned char totalExpected_byte2 = *(payload + 6);

		totalExpected = (totalExpected_byte0 << 16) | (totalExpected_byte1 << 8) | totalExpected_byte2 + 8;

		// add in extra chunk delimiters
		totalExpected += totalExpected/128;

		dataCopy = (char *)malloc(totalExpected);
	}

	if(foundStart){
		totalExpected -= sizeOfPayload;

		memcpy(dataCopy + totalFound, payload, sizeOfPayload);

		totalFound += sizeOfPayload;
	}
}

RtmpPacket * RtmpPacketAggregator::PacketReady(){
	if(totalExpected == 0 && foundStart){
		RtmpPacket * rtmpPacket = new RtmpPacket();

		rtmpPacket->data = (unsigned char *)malloc(totalFound);
		memcpy(rtmpPacket->data, dataCopy, totalFound);

		free(dataCopy);

		rtmpPacket->dataLength = totalFound;	

		// need to find the next packet 
		foundStart = false;

		return rtmpPacket;
	}
	return NULL;
}

RtmpPacket::RtmpDataTypes RtmpPacketAggregator::GetRtmpPacketType(unsigned char * data){
	unsigned char dataType = *(data + 7);

	RtmpPacket::RtmpDataTypes rtmpType = RtmpPacket::RtmpDataTypes::Unknown;
	switch(dataType){
		case 0xFF: rtmpType = RtmpPacket::RtmpDataTypes::Handshake;break;
		case 0x01: rtmpType = RtmpPacket::RtmpDataTypes::ChunkSize;break;
		case 0x04: rtmpType = RtmpPacket::RtmpDataTypes::Ping;break;
		case 0x05: rtmpType = RtmpPacket::RtmpDataTypes::ServerBandwidth;break;
		case 0x06: rtmpType = RtmpPacket::RtmpDataTypes::ClientBandwidth;break;
		case 0x08: rtmpType = RtmpPacket::RtmpDataTypes::Audio;break;
		case 0x09: rtmpType = RtmpPacket::RtmpDataTypes::Video;break;
		case 0x12: rtmpType = RtmpPacket::RtmpDataTypes::Notify;break;
		case 0x14: rtmpType = RtmpPacket::RtmpDataTypes::Invoke;break;
		case 0x16: rtmpType = RtmpPacket::RtmpDataTypes::AggregateMessage;break;
	}

	return rtmpType;
}
