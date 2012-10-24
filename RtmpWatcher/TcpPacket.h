#pragma once

#include <cstdlib>
#include <string>
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
	std::string sourceIp;
	std::string destIp;

	~TcpPacket(){ free(data); }
};