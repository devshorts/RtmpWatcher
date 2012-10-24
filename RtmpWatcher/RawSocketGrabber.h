#pragma once

#define HAVE_REMOTE
#define _CRT_SECURE_NO_WARNINGS

#include <functional>
#include <vector>
#include "pcap.h"
#include "RtmpPacket.h"
#include "PacketOrderer.h"
#include "TcpDefinitions.h"
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <WinSock2.h>
#include <windows.h>

typedef int (__stdcall *RtmpPacketFoundFuncPtr)(RtmpPacket *);

class RawSocketGrabber{
public:
	RawSocketGrabber(int deviceIndex, int targetPort);
	~RawSocketGrabber();
	void operator()();
	void Start();
	void RegisterHandler(RtmpPacketFoundFuncPtr);
	
	void Complete();

private:
	void CleanupSocket();

	void TransalteIP(unsigned int _ip, char *_cip);

	TcpPacket::TcpPacketType DeterminePacketType(unsigned short flags);

	pcap_t * InitSocket();
	
	void ReadOffSocket(pcap_t * handle);

	RtmpPacketFoundFuncPtr _rtmpPacketFoundCallback;
	
    int _targetDevice;
	int _targetPort;

	PacketOrderer orderer;

	bool isRunning;
};