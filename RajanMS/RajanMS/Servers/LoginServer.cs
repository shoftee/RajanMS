﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Network;
using RajanMS.Packets;
using RajanMS.Packets.Handlers;

namespace RajanMS.Servers
{
    class LoginServer
    {
        private Acceptor m_acceptor;
        private List<MapleClient> m_clients;
        private PacketProcessor m_processor;

        public LoginServer(short port)
        {
            m_acceptor = new Acceptor(port);
            m_acceptor.OnClientAccepted = OnClientAccepted;

            m_clients = new List<MapleClient>();

            SpawnHandlers();
        }

        private void SpawnHandlers()
        {
            m_processor = new PacketProcessor("Login");
            m_processor.AppendHandler(RecvOps.LoginPassword, LoginHandlers.HandleLoginPassword);
            m_processor.AppendHandler(RecvOps.Validate, LoginHandlers.HandleValidate);
            m_processor.AppendHandler(RecvOps.StartHackshield, GeneralHandler.HandleNothing);//to get it to shut up
        }

        private void OnClientAccepted(Socket client)
        {
            MapleClient mc = new MapleClient(client, m_processor, m_clients.Remove);
            m_clients.Add(mc);

            mc.WriteRawPacket(PacketCreator.Handshake());
            MainForm.Instance.Log("[Login] Accepted client {0}", mc.Label);
        }

        public void Run()
        {
            m_acceptor.Start();
            MainForm.Instance.Log("LoginServer listening on port {0}", m_acceptor.Port);
        }

        public void Shutdown()
        {
            m_acceptor.Stop();

            for (int i = 0; i < m_clients.Count; i++)
                m_clients[i].Close();

            m_clients.Clear();
        }
    }
}