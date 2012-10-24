
#include "RawSocketGrabber.h"

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

	auto handle = InitSocket();

	while(isRunning){
		ReadOffSocket(handle);
	}

	CleanupSocket();
}

///* Callback function invoked by libpcap for every incoming packet */
//void RawSocketGrabber::PacketCallback(u_char *param, const struct pcap_pkthdr *header, const u_char *pkt_data)
//{
//    struct tm ltime;
//    char timestr[16];
//    IPHEADER *ih;
//    //udp_header *uh;
//    u_int ip_len;
//    u_short sport,dport;
//    time_t local_tv_sec;
//
//    /*
//     * Unused variable
//     */
//    (VOID)(param);
//
//    /* convert the timestamp to readable format */
//    local_tv_sec = header->ts.tv_sec;
//    localtime_s(&ltime, &local_tv_sec);
//    strftime( timestr, sizeof timestr, "%H:%M:%S", &ltime);
//
//    /* print timestamp and length of the packet */
//    printf("%s.%.6d len:%d ", timestr, header->ts.tv_usec, header->len);
//
//    /* retireve the position of the ip header */
//    ih = (IPHEADER *) (pkt_data +
//        14); //length of ethernet header
//
//    /* retireve the position of the udp header */
//    ip_len = (ih->ver_ihl & 0xf) * 4;
//    //uh = (udp_header *) ((u_char*)ih + ip_len);
//
//    /* convert from network byte order to host byte order */
//   /* sport = ntohs( uh->sport );
//    dport = ntohs( uh->dport );*/
//
//    ///* print ip addresses and udp ports */
//    //printf("%d.%d.%d.%d.%d -> %d.%d.%d.%d.%d\n",
//    //    ih->saddr.byte1,
//    //    ih->saddr.byte2,
//    //    ih->saddr.byte3,
//    //    ih->saddr.byte4,
//    //    sport,
//    //    ih->daddr.byte1,
//    //    ih->daddr.byte2,
//    //    ih->daddr.byte3,
//    //    ih->daddr.byte4,
//    //    dport);
//}

void RawSocketGrabber::ReadOffSocket(pcap_t * adhandle){
	IPHEADER *ipHeader;
	int ipHeaderSize;

	struct pcap_pkthdr *header;
	const u_char *pkt_data;
	time_t local_tv_sec;
	int res;
	char errbuf[PCAP_ERRBUF_SIZE];
	struct tm ltime;
	char timestr[16];

	/* Retrieve the packets */
	while((res = pcap_next_ex( adhandle, &header, &pkt_data)) >= 0){

		if(res == 0)
			/* Timeout elapsed */
			continue;

		/* convert the timestamp to readable format */
		local_tv_sec = header->ts.tv_sec;
		localtime_s(&ltime, &local_tv_sec);
		strftime( timestr, sizeof timestr, "%H:%M:%S", &ltime);

		printf("%s,%.6d len:%d\n", timestr, header->ts.tv_usec, header->len);

		//// Read http://www.ietf.org/rfc/rfc1700.txt?number=1700
		//switch( ipHeader->protocol )
		//{
		//	case 6: // TCP
		//	{ 
		//		/*std::string target = "10.0.6.14";
		//		std::string source(ipSrc);*/
		//	
		//	
		//		//char * tcpHeaderStart = &packet[ipHeaderSize];
		//	
		//		//if(TargetPortFound(tcpHeaderStart)){
		//			/*if(source.compare(target) == 0){
		//				printf("read bytes %d from %s to %s\n", bytesRead, ipSrc, ipDest);
		//			}*/
		//		
		//			/*orderer.AddPacket(packet, bytesRead);

		//			RtmpPacket * rtmpPacket = orderer.PacketReady();

		//			if(rtmpPacket != NULL){
		//				rtmpPacket->ipHeader = ipHeader;
		//				rtmpPacket->tcpHeader = (TCPHEADER *)tcpHeaderStart;
		//				rtmpPacket->sourceIp = ipSrc;
		//				rtmpPacket->destIp = ipDest;

		//				_rtmpPacketFoundCallback(rtmpPacket);

		//				delete rtmpPacket;
		//			}*/
		//		//}

		//		//break;
		//	}
		//}

		//delete packet;

	}

	if(res == -1){
		printf("Error reading the packets: %s\n", pcap_geterr(adhandle));
		return;
	}
}


void RawSocketGrabber::CleanupSocket(){

}

void RawSocketGrabber::operator ()(){
	Start();
}

void RawSocketGrabber::Complete(){
	isRunning = false;
}

void RawSocketGrabber::RegisterHandler(RtmpPacketFoundFuncPtr handler){
	_rtmpPacketFoundCallback = handler;
}

pcap_t * RawSocketGrabber::InitSocket(){
	pcap_if_t *alldevs;
	pcap_if_t *d;
	int inum;
	int i=0;
	pcap_t *adhandle;
	int res;
	char errbuf[PCAP_ERRBUF_SIZE];
	struct tm ltime;
	char timestr[16];
	struct pcap_pkthdr *header;
	const u_char *pkt_data;
	time_t local_tv_sec;

	/* Retrieve the device list on the local machine */
	if (pcap_findalldevs_ex(PCAP_SRC_IF_STRING, NULL, &alldevs, errbuf) == -1)
	{
		fprintf(stderr,"Error in pcap_findalldevs: %s\n", errbuf);
		exit(1);
	}

	/* Print the list */
	for(d=alldevs; d; d=d->next)
	{
		printf("%d. %s", ++i, d->name);
		if (d->description)
			printf(" (%s)\n", d->description);
		else
			printf(" (No description available)\n");
	}

	if(i==0)
	{
		printf("\nNo interfaces found! Make sure WinPcap is installed.\n");
		return NULL;
	}

	printf("Enter the interface number (1-%d):",i);
	scanf_s("%d", &inum);

	if(inum < 1 || inum > i)
	{
		printf("\nInterface number out of range.\n");
		/* Free the device list */
		pcap_freealldevs(alldevs);
		return NULL;
	}

	/* Jump to the selected adapter */
	for(d=alldevs, i=0; i< inum-1 ;d=d->next, i++);

	/* Open the device */
	if ( (adhandle= pcap_open(d->name,          // name of the device
		LS_MAX_PACKET_SIZE,            // portion of the packet to capture. 
		// 65536 guarantees that the whole packet will be captured on all the link layers
		PCAP_OPENFLAG_PROMISCUOUS,    // promiscuous mode
		1000,             // read timeout
		NULL,             // authentication on the remote machine
		errbuf            // error buffer
		) ) == NULL)
	{
		fprintf(stderr,"\nUnable to open the adapter. %s is not supported by WinPcap\n", d->name);
		/* Free the device list */
		pcap_freealldevs(alldevs);
		return NULL;
	}

	printf("\nlistening on %s...\n", d->description);

	/* At this point, we don't need any more the device list. Free it */
	pcap_freealldevs(alldevs);

	return adhandle;
}

void RawSocketGrabber::CreatePromisciousSocket(){
	//int optval = SIO_RCVALL;
	//DWORD dwLen = 0;
	//// Set socket to promiscuous mode
	//// setsockopt wont work ... dont even try it
	//if ( WSAIoctl( socketPtr, 
	//	SIO_RCVALL,
	//	&optval,
	//	sizeof(optval),
	//	NULL,
	//	0,
	//	&dwLen,
	//	NULL,
	//	NULL ) == SOCKET_ERROR )

	//{
	//	printf( "Error setting promiscious mode: WSAIoctl  = %ld\n", WSAGetLastError() );
	//	throw "Error setting promsocous mode";
	//}

}

void RawSocketGrabber::BindSocketToIp(){
//	char localIp[20];
//
//	memset( localIp, 0x00, sizeof(localIp) );
//
//	GetMachineIP(localIp);
//
//	printf("using ip %s", localIp);
//
////	socketDefinition.sin_family = AF_INET;
//
//	socketDefinition.sin_port = htons(50000);
//
//	// If your machine has more than one IP you might put another one instead thisIP value
//	socketDefinition.sin_addr.s_addr = inet_addr(localIp);
//
//	if ( bind( socketPtr, (struct sockaddr *)&socketDefinition, sizeof(socketDefinition) ) == SOCKET_ERROR )
//	{
//		printf( "Error: bind = %ld\n", WSAGetLastError() );
//		throw "Error binding";
//	}
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

bool RawSocketGrabber::TargetPortFound(char *packet)
{
	TCPHEADER *tcp_header = (TCPHEADER *)packet;
	
	if(htons(tcp_header->source_port) == _targetPort){
		return true;
	}

	return false;
}

TcpPacket::TcpPacketType RawSocketGrabber::DeterminePacketType(unsigned short flags){
	if ( flags & 0x01 ) // FIN
		return TcpPacket::FIN;

	if ( flags & 0x02 ) // SYN
		return TcpPacket::SYN;

	if ( flags & 0x04 ) // RST
		return TcpPacket::RST;

	if ( flags & 0x08 ) // PSH
		return TcpPacket::PSH;

	if ( flags & 0x10 ) // ACK
		return TcpPacket::ACK;

	if ( flags & 0x20 ) // URG
		return TcpPacket::URG;
}