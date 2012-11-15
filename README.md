RtmpWatcher
===========

This is a basic tool that captures Rtmp AMF packets using WinPCap, reassmbles them by destination port, and parsers the raw AMF into a parseable container.  This lets you sit in the middle of an Rtmp conversation and see the typed data go through. 

The basic idea
===========

RawSocketGrabber.cpp is responsible for capturing data via WinPCap from the NIC. It has a built in filter of "tcp and src port {0} or dest port {1}". The port is passed in via C#.  THe socket grabber takes a packet and sends it to the "orderer". The orderer (PacketOrderer.cpp) is responsible for organizing packets by destination ports. The aggregator (RtmpPacketAggregator.cpp) pieces together tcp packets and parsers raw rtmp packets out of. Every time you capture a packet in the grabber, the grabber then asks to see if there are any packets that are ready (via the orderer to the aggregator). When a packet is ready an RtmpPacket object. Once we have a packet, it gets marshalled through managed via a function pointer to an interop class (RtmpWatcherInterop.cpp) that invokes an Action<RtmpPacketInterop> delegate into C#. 

http://tech.blinemedical.com/dropped-packets-with-promiscuous-raw-sockets/