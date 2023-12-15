using Godot;

public partial class QueueFreeThis : Node3D
{

	public void OnArea3dBodyEntered(Node3D body)
	{
		if(!Multiplayer.IsServer()) return;

		if(body.IsInGroup("Players"))
		{
			Rpc(nameof(Destroy));
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Destroy()
	{
		QueueFree();
	}
}