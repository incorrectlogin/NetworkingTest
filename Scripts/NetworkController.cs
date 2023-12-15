using Godot;
using System.Collections.Generic;

public partial class NetworkController : Node
{
	[Export]public ENetConnection.CompressionMode compression = ENetConnection.CompressionMode.RangeCoder;
	[Export]public PackedScene playerCharacter;
	private ENetMultiplayerPeer peer;
	const int PORT = 6969;
	const string IP = "127.0.0.1";
	public int MAX_CLIENTS = 8;
	public List<long> Players = new List<long>();
	int index = 0;
	PlayerCharacter currentPlayer;

	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

    private void ConnectionFailed()
    {
        GD.Print("Connection Failed");
    }

    private void ConnectedToServer()
    {
		if(!Multiplayer.IsServer()) GD.Print("(Client) Connected to Server");
		if(Multiplayer.IsServer()) GD.Print("(Server) Connected to Server");
		RpcId(1, "AddPlayerInformation", Multiplayer.GetUniqueId());
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
    }

	private void PeerConnected(long id)
	{
		if(!Multiplayer.IsServer()) GD.Print("(Client) Peer Connected " + Multiplayer.GetUniqueId());
		if(Multiplayer.IsServer()) GD.Print("(Server) Peer Connected " + Multiplayer.GetUniqueId());

		if(!Multiplayer.IsServer()) GD.Print("(Client) Player Connected: " + id.ToString());
		if(Multiplayer.IsServer()) GD.Print("(Server) Player Connected: " + id.ToString());
		SpawnPlayer(id);
	}

    public void HostServer()
	{
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(PORT, MAX_CLIENTS);

		if(error != Error.Ok)
		{
			GD.Print("Cannot start Host: " + error.ToString());
			return;
		}
		
		peer.Host.Compress(compression);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("You are now hosting.");
		AddPlayerInformation(Multiplayer.GetUniqueId());
		var newNode = new Marker3D();
		GetParent().AddChild(newNode);
		newNode.Name = "Server";
	}

	public void JoinServer()
	{
		peer = new ENetMultiplayerPeer();
		
		var error = peer.CreateClient(IP, PORT);
		if(error != Error.Ok)
		{
			GD.Print("Cannot join Host: " + error.ToString());
			return;
		}
		peer.Host.Compress(compression);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("You are now joining the server.");
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void AddPlayerInformation(long id)
	{
		Players.Add(id);
		SpawnPlayer(id);

		if(Multiplayer.IsServer())
		{
			foreach (var item in Players)
			{
				Rpc(nameof(AddPlayerInformation), item);
			}
		}
	}

	/// Menu button methods
	public void OnHostPressed()
	{
		HostServer();
	}

	public void OnJoinPressed()
	{
		JoinServer();
	}

	public void SpawnPlayer(long id)
	{
		var spawnPoints = GetTree().GetNodesInGroup("Spawnpoints");
		
		GD.Print("Spawning player " + id);
		if(GetNodeOrNull(id.ToString()) == null)
		{
			currentPlayer = playerCharacter.Instantiate<PlayerCharacter>();
			currentPlayer.Name = id.ToString();
			AddChild(currentPlayer);
		
			Marker3D sp = (Marker3D)spawnPoints[index];
			currentPlayer.GlobalPosition = sp.GlobalPosition;
			currentPlayer.GlobalRotation = sp.GlobalRotation;
			index++;
			if(index > Players.Count)
				index = 0;
		}

		foreach (var item in Players)
		{
			//currentPlayer.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor((int)item, true);
		}
	}
}