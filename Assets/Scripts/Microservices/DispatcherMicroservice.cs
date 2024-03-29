﻿using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class DispatcherMicroservice : Microservice<GetDispatcherRequest,
        PostServerRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        public void RequestServerInfo(string gameID, UnityAction<ServerInfo> onServerInfo, UnityAction<string> onFail)
        {
            this.Request(new GetServerInfoRequest(gameID, onServerInfo, onFail));
        }

        public void RequestNewServerInfo(UnityAction<ServerInfo> onServerInfo, UnityAction<string> onFail)
        {
            this.Request(new GetNewServerInfoRequest(onServerInfo, onFail));
        }

        public void RequestNewServer(UnityAction<ServerInfo> onServerInfo, UnityAction<string> onFail)
        {
            this.Request(new PostServerRequest(onServerInfo, onFail));
        }

        [System.Serializable]
        private struct JSONServerInfo
        {
            public string server_id;
            public string server_ip;
            public int tcp_port;
            public int udp_port;
        }

        protected override void OnGetResponse(string JSON, GetDispatcherRequest originalRequest)
        {
            Debug.Log("Received response");
            JSONServerInfo jsonInfo = JsonUtility.FromJson<JSONServerInfo>(JSON);
            ServerInfo serverInfo = new ServerInfo(jsonInfo.server_id, jsonInfo.server_ip, jsonInfo.server_ip, jsonInfo.tcp_port, jsonInfo.udp_port);

            originalRequest.SuccessCallback.Invoke(serverInfo);
        }

        protected override void OnPostResponse(string JSON, PostServerRequest originalRequest)
        {
            JSONServerInfo jsonInfo = JsonUtility.FromJson<JSONServerInfo>(JSON);
            ServerInfo serverInfo = new ServerInfo(jsonInfo.server_id, jsonInfo.server_ip, jsonInfo.server_ip, jsonInfo.tcp_port, jsonInfo.udp_port);

            originalRequest.Success?.Invoke(serverInfo);
        }

        protected override void MockGet(GetDispatcherRequest request)
        {
            if (request is GetNewServerInfoRequest) {
                ServerInfo serverInfo = new ServerInfo(
                    m_mockData.GameID,
                    m_mockData.ServerTCPAddress,
                    m_mockData.ServerUDPAddress,
                    m_mockData.ServerTCPPort,
                    m_mockData.ServerUDPPort);

                request.SuccessCallback?.Invoke(serverInfo);
            }
            else if (request is GetServerInfoRequest)
            {
                if ((request as GetServerInfoRequest).GameID.Equals(m_mockData.GameID))
                {
                    ServerInfo serverInfo = new ServerInfo(
                        (request as GetServerInfoRequest).GameID,
                        m_mockData.ServerTCPAddress,
                        m_mockData.ServerUDPAddress,
                        m_mockData.ServerTCPPort,
                        m_mockData.ServerUDPPort);

                    request.SuccessCallback?.Invoke(serverInfo);
                }
                else
                {
                    request.SuccessCallback?.Invoke(null);
                }
            }

        }

        protected override void MockPost(PostServerRequest request)
        {
            ServerInfo serverInfo = new ServerInfo(
                m_mockData.GameID,
                m_mockData.ServerTCPAddress,
                m_mockData.ServerUDPAddress,
                m_mockData.ServerTCPPort,
                m_mockData.ServerUDPPort);

            request.Success?.Invoke(serverInfo);
        }
    }
}
