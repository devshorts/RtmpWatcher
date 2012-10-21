#pragma once

#include "TcpDefinitions.h"

class TcpPacket{
public:
	enum TcpPacketType{
		FIN,
		SYN,
		RST,
		PSH,
		ACK,
		URG
	};

	TCPHEADER * tcpHeader;
	IPHEADER * ipHeader;
	unsigned char * data;
	int dataLength;
	char * sourceIp;
	char * destIp;
};