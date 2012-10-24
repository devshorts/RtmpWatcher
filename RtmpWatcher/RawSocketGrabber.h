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
#include <string.h>
#include <WinSock2.h>
#include <windows.h>

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

	pcap_t * InitSocket();
	void BindSocketToIp();
	void CreatePromisciousSocket();

	void ReadOffSocket(pcap_t * handle);

	//void PacketCallback(u_char *param, const struct pcap_pkthdr *header, const u_char *pkt_data);

	//SocketData * ParseData(unsigned char * data);

	RtmpPacketFoundFuncPtr _rtmpPacketFoundCallback;

	
	int _targetPort;	

	PacketOrderer orderer;

	bool isRunning;
};