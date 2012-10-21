#pragma once

#include <functional>
#include <vector>
#include "RtmpPacket.h"
#include "RtmpPacketAggregator.h"

typedef int (__stdcall *RtmpPacketFoundFuncPtr)(RtmpPacket *);

class RawSocketGrabber{
public:
	RawSocketGrabber(int targetPort);
	~RawSocketGrabber();
	void operator()();
	void Start();
	void RegisterHandler(RtmpPacketFoundFuncPtr);
	
	void Complete();

private:
	void CleanupSocket();

	void GetMachineIP(char * ip);
	void TransalteIP(unsigned int _ip, char *_cip);
	bool TargetPortFound(char *_packet);

	TcpPacket::TcpPacketType DeterminePacketType(unsigned short flags);

	void InitSocket();
	void BindSocketToIp();
	void CreatePromisciousSocket();

	void ReadOffSocket();

	//SocketData * ParseData(unsigned char * data);

	RtmpPacketFoundFuncPtr _rtmpPacketFoundCallback;

	
	int _targetPort;	
	SOCKET socketPtr;
	sockaddr_in socketDefinition;
	RtmpPacketAggregator aggregator;

	bool isRunning;
};