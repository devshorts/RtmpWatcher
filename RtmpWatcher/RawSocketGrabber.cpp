#include <iostream>
#include <winsock2.h>
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <Ws2tcpip.h>
#include "Mstcpip.h"

#include "RawSocketGrabber.h"
#include "TcpDefinitions.h"
#include "SocketData.h"

#define LS_MAX_PACKET_SIZE 65535

#define High4Bits(x)  ((x>>4) & 0x0F)

#define Low4Bits(x)  ((x) & 0x0F)

#pragma comment( lib, "ws2_32.lib" ) // linker must use this lib for sockets

RawSocketGrabber::RawSocketGrabber(int targetPort){
	_targetPort = targetPort;
	isRunning = false;
}

RawSocketGrabber::~RawSocketGrabber(){

}

void RawSocketGrabber::Start(){
	isRunning = true;

	InitSocket();

	while(isRunning){
		ReadOffSocket();
	}

	CleanupSocket();
}

void RawSocketGrabber::ReadOffSocket(){
	IPHEADER *ipHeader;
	int ipHeaderSize;

	char* packet = new char[LS_MAX_PACKET_SIZE];
	char  ipSrc[20], ipDest[20];

	memset( packet, 0, sizeof(packet) );
	memset( ipSrc, 0, sizeof(ipSrc) );
	memset( ipDest, 0, sizeof(ipDest) );

	int bytesRead = recv( socketPtr, packet, LS_MAX_PACKET_SIZE, 0 );

	printf("\nRead data from socket %d", bytesRead);

	if ( bytesRead < sizeof(IPHEADER) ){
		delete packet;
		return;
	}

	ipHeader = (IPHEADER *)packet;

	// I only want IPv4 not IPv6
	if ( High4Bits(ipHeader->ver_ihl) != 4 ){
		delete packet;
		return;
	}

	ipHeaderSize = Low4Bits(ipHeader->ver_ihl);
	ipHeaderSize *= sizeof(DWORD); // size in 32 bits words

	TransalteIP(ipHeader->source_ip, ipSrc);
	TransalteIP(ipHeader->destination_ip, ipDest);

	// Read http://www.ietf.org/rfc/rfc1700.txt?number=1700
	switch( ipHeader->protocol )
	{
		case 6: // TCP
		{ 
				printf("\n -------------------- // -------------------- ");
				printf("\n IP Header:");
				printf("\n   Source      IP: %s", ipSrc);
				printf("\n   Destination IP: %s", ipDest);
				printf("\n      TCP Header:");
				printf("\n			Offset  : %d", ipHeader->flags_foff & 0xE0);
				printf("\n			PacketID: %d", ipHeader->packet_id);

				DecodeTcp(&packet[ipHeaderSize]);

				break;
		}
	}

	delete packet;
}


void RawSocketGrabber::CleanupSocket(){
	if(closesocket(socketPtr) != 0){
		printf("Error closing socket");
	}
}

void RawSocketGrabber::operator ()(){
	Start();
}

void RawSocketGrabber::Complete(){
	isRunning = false;
}

void RawSocketGrabber::RegisterHandler(std::function<void (SocketData *)> handler){
	_packetFoundHandler = handler;
}

void RawSocketGrabber::InitSocket(){

	// load winsock dll with wsa startup

	WSAData wsaData;	
	auto ver = MAKEWORD(2,2);
	if(WSAStartup(ver, &wsaData) != 0){
		printf("Error initializing wsastartup");
		throw "Error initializing wsa startup";
	}

	// Get a socket in RAW mode
	socketPtr = socket( AF_INET, SOCK_RAW, IPPROTO_IP   );
	if (socketPtr == SOCKET_ERROR )
	{
		printf("Error: socket = %ld\n", WSAGetLastError());
		throw "Error opening socket";
	}

	//int optval=1;
	
	//setsockopt(socketPtr, IPPROTO_IP, IP_HDRINCL, (char *)&optval, sizeof optval);

	BindSocketToIp();

	CreatePromisciousSocket();
}

void RawSocketGrabber::CreatePromisciousSocket(){
	int optval = 1;
	DWORD dwLen = 0;
	// Set socket to promiscuous mode
	// setsockopt wont work ... dont even try it
	if ( WSAIoctl( socketPtr, 
		SIO_RCVALL,
		&optval,
		sizeof(optval),
		NULL,
		0,
		&dwLen,
		NULL,
		NULL ) == SOCKET_ERROR )

	{
		printf( "Error setting promiscious mode: WSAIoctl  = %ld\n", WSAGetLastError() );
		throw "Error setting promsocous mode";
	}

}

void RawSocketGrabber::BindSocketToIp(){
	char localIp[20];

	memset( localIp, 0x00, sizeof(localIp) );

	GetMachineIP(localIp);

	socketDefinition.sin_family = AF_INET;

	socketDefinition.sin_port = htons(50000);

	// If your machine has more than one IP you might put another one instead thisIP value
	socketDefinition.sin_addr.s_addr = inet_addr(localIp);

	if ( bind( socketPtr, (struct sockaddr *)&socketDefinition, sizeof(socketDefinition) ) == SOCKET_ERROR )
	{
		printf( "Error: bind = %ld\n", WSAGetLastError() );
		throw "Error binding";
	}
}

void RawSocketGrabber::GetMachineIP(char *ip)
{
	char host_name[128];
	struct hostent *hs;
	struct in_addr in;

	memset( host_name, 0x00, sizeof(host_name) );
	gethostname(host_name,128);
	hs = gethostbyname(host_name);

	memcpy( &in, hs->h_addr, hs->h_length );
	strcpy( ip, inet_ntoa(in) );
}

void RawSocketGrabber::TransalteIP(unsigned int _ip, char *_cip)
{
	struct in_addr in;

	in.S_un.S_addr = _ip;

	strcpy( _cip, inet_ntoa(in) );
}

void RawSocketGrabber::DecodeTcp(char *packet)
{
	TCPHEADER *tcp_header = (TCPHEADER *)packet;
	unsigned short flags = ( ntohs(tcp_header->info_ctrl) & 0x003F ); 

	printf("\n         source port      : %ld", htons(tcp_header->source_port));
	printf("\n         destination port : %ld", htons(tcp_header->destination_port));
	printf("\n         control bits     : ");

	TcpPacketType packetType = DeterminePacketType(flags);

	printf("\n         sequence number  : %lu", ntohl(tcp_header->seq_number));
}

RawSocketGrabber::TcpPacketType RawSocketGrabber::DeterminePacketType(unsigned short flags){
	if ( flags & 0x01 ) // FIN
		return TcpPacketType::FIN;

	if ( flags & 0x02 ) // SYN
		return TcpPacketType::SYN;

	if ( flags & 0x04 ) // RST
		return TcpPacketType::RST;

	if ( flags & 0x08 ) // PSH
		return TcpPacketType::PSH;

	if ( flags & 0x10 ) // ACK
		return TcpPacketType::ACK;

	if ( flags & 0x20 ) // URG
		return TcpPacketType::URG;
}