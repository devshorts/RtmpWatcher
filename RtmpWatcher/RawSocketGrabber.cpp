#include <iostream>
#include <winsock2.h>
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "RawSocketGrabber.h"
#include "TcpDefinitions.h"
#include "SocketData.h"

#pragma comment( lib, "ws2_32.lib" ) // linker must use this lib for sockets

RawSocketGrabber::RawSocketGrabber(int targetPort){
	_targetPort = targetPort;
}

void RawSocketGrabber::Start(){
	
}

void RawSocketGrabber::operator ()(){
	Start();
}

void RawSocketGrabber::RegisterHandler(std::function<void (SocketData *)> handler){
	_packetFoundHandler = handler;
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

void RawSocketGrabber::DecodeTcp(char *_packet)
{
	TCPHEADER *tcp_header = (TCPHEADER *)_packet;
	BYTE flags = ( ntohs(tcp_header->info_ctrl) & 0x003F ); 

	printf("\n         source port      : %ld", htons(tcp_header->source_port));
	printf("\n         destination port : %ld", htons(tcp_header->destination_port));
	printf("\n         control bits     : ");

	if ( flags & 0x01 ) // FIN
		printf( "FIN " );

	if ( flags & 0x02 ) // SYN
		printf( "SYN " );

	if ( flags & 0x04 ) // RST
		printf( "RST " );

	if ( flags & 0x08 ) // PSH
		printf( "PSH " );

	if ( flags & 0x10 ) // ACK
		printf( "ACK " );

	if ( flags & 0x20 ) // URG
		printf( "URG " );

	printf("\n         sequence number  : %lu", ntohl(tcp_header->seq_number));
}