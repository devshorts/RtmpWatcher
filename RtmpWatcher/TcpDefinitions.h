#pragma once

#include <WinSock2.h>
#include <windows.h>

typedef struct _IP_HEADER_
{
	unsigned char  ver_ihl;        // Version (4 bits) and Internet Header Length (4 bits)
	unsigned char  type;           // Type of Service (8 bits)
	unsigned short  length;         // Total size of packet (header + data)(16 bits)
	unsigned short  packet_id;      // (16 bits)
	unsigned short  flags_foff;     // Flags (3 bits) and Fragment Offset (13 bits)
	unsigned char  time_to_live;   // (8 bits)
	unsigned char  protocol;       // (8 bits)
	unsigned short  hdr_chksum;     // Header check sum (16 bits)
	unsigned int source_ip;      // Source Address (32 bits)
	unsigned int destination_ip; // Destination Address (32 bits)
} IPHEADER;

typedef struct _TCP_HEADER_
{
	unsigned short  source_port;       // (16 bits)
	unsigned short  destination_port;  // (16 bits)
	unsigned int seq_number;        // Sequence Number (32 bits)
	unsigned int ack_number;        // Acknowledgment Number (32 bits)
	unsigned short  info_ctrl;         // Data Offset (4 bits), Reserved (6 bits), Control bits (6 bits)
	unsigned short  window;            // (16 bits)
	unsigned short  checksum;          // (16 bits)
	unsigned short  urgent_pointer;    // (16 bits)
} TCPHEADER;

typedef struct _ICMP_HEADER_
{
	unsigned char type;               // (8 bits)  
	unsigned char code;               // (8 bits)  
	unsigned short checksum;           // (16 bits)  
} ICMPHEADER;
