﻿using UnityEngine;
using UnityEngine.UI;
using RetardedNetworking;

public class GameStatusManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject puppetPrefab;
    public Transform spawnPoint;
    public Text textServer;
    public Text textClient;
    public Text textHost;

    //private void Awake()
    //{
    //    Application.targetFrameRate = 10;
    //}

    //private void Start()
    //{
    //    Application.targetFrameRate = 10;
    //}

    // Update is called once per frame
    private void Update()
    {

        if (Input.GetKeyUp(KeyCode.S))
            ToggleServer();

        if (Input.GetKeyUp(KeyCode.C))
            ToggleClient();

        if (Input.GetKeyUp(KeyCode.H))
            ToggleHost();

        NetworkManager n = NetworkManager.Singleton;
        if (n == null) return;

        textServer.text = "IsServer = " + (n.IsServer);
        textServer.color = n.IsServer ? Color.green : Color.red;
        textClient.text = "IsClient = " + (n.IsClient);
        textClient.color = n.IsClient ? Color.green : Color.red;
        textHost.text = "IsHost = " + (n.IsServer && n.IsClient);
        textHost.color = n.IsHost ? Color.green : Color.red;
    }

    private void ToggleServer()
    {
        NetworkManager n = NetworkManager.Singleton;
        if (n == null) return;

        if (n.IsServer)
            n.StopServer();
        else
            n.StartServer();
    }

    private void ToggleClient()
    {
        NetworkManager n = NetworkManager.Singleton;
        if (n == null) return;

        if (n.IsClient)
            n.StopClient();
        else
            n.StartClient();
    }

    public void ToggleHost()
    {
        NetworkManager n = NetworkManager.Singleton;
        if (n == null) return;

        if (n.IsHost)
            n.StopHost();
        else
            n.StartHost();
    }
}
