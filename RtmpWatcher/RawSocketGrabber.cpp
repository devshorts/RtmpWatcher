
#include "RawSocketGrabber.h"

#define LS_MAX_PACKET_SIZE 65535

#define High4Bits(x)  ((x>>4) & 0x0F)

#define Low4Bits(x)  ((x) & 0x0F)

#pragma comment( lib, "ws2_32.lib" ) // linker must use this lib for sockets

RawSocketGrabber::RawSocketGrabber(int deviceIndex, int targetPort){
    _targetDevice = deviceIndex;
	_targetPort = targetPort;
	isRunning = false;
}

RawSocketGrabber::~RawSocketGrabber(){

}

void RawSocketGrabber::Start(){
	isRunning = true;

	auto handle = InitSocket();

	if(handle == NULL){
		return;
	}

	while(isRunning){
		ReadOffSocket(handle);
	}

	CleanupSocket();
}

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
	const int ethernetFrameHeader = 14;

	/* Retrieve the packets */
	while((res = pcap_next_ex( adhandle, &header, &pkt_data)) >= 0 && isRunning){

		if(res == 0){
			/* Timeout elapsed */
			continue;
		}

		orderer.AddPacket((const char *)(pkt_data + ethernetFrameHeader), header->len - ethernetFrameHeader);

		RtmpPacket * rtmpPacket = orderer.PacketReady();

		if(rtmpPacket != NULL){
			char srcIp[20];
			char dstIp[20];
			TransalteIP(((IPHEADER *)(pkt_data + ethernetFrameHeader))->source_ip, srcIp);
			TransalteIP(((IPHEADER *)(pkt_data + ethernetFrameHeader))->destination_ip, dstIp);

			rtmpPacket->sourceIp.append(srcIp);
			rtmpPacket->destIp.append(dstIp);

			_rtmpPacketFoundCallback(rtmpPacket);

			delete rtmpPacket;
		}
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
	std::string targetPortString = std::to_string(static_cast<long long>(_targetPort));
	std::string packetFilter("tcp and src port " + targetPortString);
	
	u_int netmask;
	struct bpf_program fcode;

	/* Retrieve the device list on the local machine */
	if (pcap_findalldevs_ex(PCAP_SRC_IF_STRING, NULL, &alldevs, errbuf) == -1)
	{
		fprintf(stderr,"Error in pcap_findalldevs: %s\n", errbuf);
		exit(1);
	}

    inum = _targetDevice;

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

	if(d->addresses != NULL){
		/* Retrieve the mask of the first address of the interface */
		netmask=((struct sockaddr_in *)(d->addresses->netmask))->sin_addr.S_un.S_addr;
	}
	else{
		/* If the interface is without addresses we suppose to be in a C class network */
		netmask=0xffffff;
	}

	printf("\nlistening on %s...\n", d->description);

	if (pcap_compile(adhandle, &fcode, packetFilter.c_str(), 1, netmask) < 0)
	{
		fprintf(stderr,"\nUnable to compile the packet filter. Check the syntax.\n");
		/* Free the device list */
		pcap_freealldevs(alldevs);
		return NULL;
	}

	//set the filter
	if (pcap_setfilter(adhandle, &fcode)<0)
	{
		fprintf(stderr,"\nError setting the filter.\n");
		/* Free the device list */
		pcap_freealldevs(alldevs);
		return NULL;
	}

	return adhandle;
}

void RawSocketGrabber::TransalteIP(unsigned int _ip, char *_cip)
{
	struct in_addr in;

	in.S_un.S_addr = _ip;

	strcpy( _cip, inet_ntoa(in) );
}
