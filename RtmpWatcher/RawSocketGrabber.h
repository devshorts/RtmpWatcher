#pragma once

#include <functional>
#include <vector>
#include "SocketData.h"

typedef int (__stdcall *dataReceivedFuncPtr)(unsigned char *, int);

class RawSocketGrabber{
public:

	enum TcpPacketType{
		FIN,
		SYN,
		RST,
		PSH,
		ACK,
		URG
	};

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

	RawSocketGrabber(int targetPort);
	~RawSocketGrabber();
	void operator()();
	void Start();
	void RegisterHandler(dataReceivedFuncPtr);
	
	void Complete();

private:
	void CleanupSocket();

	void GetMachineIP(char * ip);
	void TransalteIP(unsigned int _ip, char *_cip);
	bool DecodeTcp(char *_packet);
	TcpPacketType DeterminePacketType(unsigned short flags);

	void InitSocket();
	void BindSocketToIp();
	void CreatePromisciousSocket();

	bool ParseRtmpPacket(unsigned char * data);

	void ReadOffSocket();

	//SocketData * ParseData(unsigned char * data);

	dataReceivedFuncPtr _packetFoundHandler;

	
	int _targetPort;	
	SOCKET socketPtr;
	sockaddr_in socketDefinition;

	bool isRunning;
};