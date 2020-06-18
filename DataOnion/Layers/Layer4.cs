using System;
using System.Collections.Generic;
using System.IO;

namespace DataOnion.Layers
{
    public static class Layer4
    {
        const string ExpectedFromIp = "10.1.1.10";
        const string ExpectedToIp = "10.1.1.200";
        const int ExpectedToPort = 42069;

        public static byte[] NetworkStreamToPayload(byte[] bytes)
        {
            var networkStream = new MemoryStream(bytes);
            var validPackets = NetworkStreamToPackets(networkStream);

            var outStream = new MemoryStream();
            foreach (var packet in validPackets)
            {
                outStream.Write(packet.Payload);
            }

            // *** Note from author ***
            // Fairly sure this output is correct but not quite 100% sure
            // Will find out when working on the Layer 5 solution
            return outStream.ToArray();
        }

        public static List<Packet> NetworkStreamToPackets(Stream stream)
        {
            var packets = new List<Packet>();
            while (stream.Position < stream.Length)
            {
                var packet = new Packet(stream);
                if (PacketValidatesRules(packet))
                {
                    packets.Add(packet);
                }
            }
            return packets;
        }

        static bool PacketValidatesRules(Packet packet)
        {
            //This function could be much less verbose but found it handy like this for debugging
            var valid = true;

            if (packet.SrcIp != ExpectedFromIp)
            {
                valid = false;
            }

            if (packet.DstIp != ExpectedToIp)
            {
                valid = false;
            }

            if (packet.DstPort != ExpectedToPort)
            {
                valid = false;
            }

            if (!packet.IPv4HeaderCorrect)
            {
                valid = false;
            }

            if (!packet.UdpHeaderCorrect)
            {
                valid = false;
            }

            return valid;
        }
    }

    public class Packet
    {
        const ushort IpHeaderLength = 20;
        const ushort UdpHeaderLength = 8;

        public string SrcIp { get; set; }
        public string DstIp { get; set; }
        public int SrcPort { get; set; }
        public int DstPort { get; set; }
        public byte[] Payload { get; set; }
        public bool IPv4HeaderCorrect { get; set; }
        public bool UdpHeaderCorrect { get; set; }

        public Packet(Stream stream)
        {
            //Read IP data
            var ipHeader = ReadData(stream, IpHeaderLength);
            var ipHeaderStream = new MemoryStream(ipHeader);
            ReadData(ipHeaderStream, 9); //Discard
            var ipProtocol = ReadData(ipHeaderStream, 1);
            ReadData(ipHeaderStream, 2); //Discard
            var ipSrc = ReadData(ipHeaderStream, 4);
            var ipDst = ReadData(ipHeaderStream, 4);

            //Read UDP data
            var udpHeader = ReadData(stream, UdpHeaderLength);
            var udpHeaderStream = new MemoryStream(udpHeader);
            var udpSrcPort = ReadUint16(udpHeaderStream);
            var udpDstPort = ReadUint16(udpHeaderStream);
            var streamPos = udpHeaderStream.Position;
            var udpLength = ReadUint16(udpHeaderStream);
            udpHeaderStream.Position = streamPos;
            var udpLengthBytes = ReadData(udpHeaderStream, 2);

            //Set object values
            SrcIp = $"{ipSrc[0]}.{ipSrc[1]}.{ipSrc[2]}.{ipSrc[3]}";
            SrcPort = udpSrcPort;
            DstIp = $"{ipDst[0]}.{ipDst[1]}.{ipDst[2]}.{ipDst[3]}";
            DstPort = udpDstPort;
            Payload = ReadData(stream, udpLength - UdpHeaderLength);

            //Calculate IP checksum correct
            IPv4HeaderCorrect = ValidHeaderChecksum(new MemoryStream(ipHeader));

            //Prepare UDP checksum data
            var udpChecksumData = new byte[12 + udpLength];
            ipSrc.CopyTo(udpChecksumData, 0);
            ipDst.CopyTo(udpChecksumData, 4);
            ipProtocol.CopyTo(udpChecksumData, 9);
            udpLengthBytes.CopyTo(udpChecksumData, 10);
            udpHeader.CopyTo(udpChecksumData, 12);
            Payload.CopyTo(udpChecksumData, 20);

            //Calculate UDP checksum correct
            UdpHeaderCorrect = ValidHeaderChecksum(new MemoryStream(udpChecksumData));
        }

        bool ValidHeaderChecksum(Stream stream)
        {
            uint count = 0;

            while (stream.Position < stream.Length)
            {
                count += ReadUint16(stream);
            }

            if (count > ushort.MaxValue)
            {
                var carry = count >> 16;
                count %= ushort.MaxValue + 1;
                count += carry;
            }

            var checksumValid = count == ushort.MaxValue;

            return checksumValid;
        }

        byte[] ReadData(Stream stream, int length)
        {
            var data = new byte[length];
            stream.Read(data, 0, data.Length);
            return data;
        }

        ushort ReadUint16(Stream stream)
        {
            var data = ReadData(stream, 2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            var i = BitConverter.ToUInt16(data);
            return i;
        }

    }
}
