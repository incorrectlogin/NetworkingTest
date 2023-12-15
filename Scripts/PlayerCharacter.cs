using Godot;

public partial class PlayerCharacter : CharacterBody3D
{
	public float speed = 5.0f;
	public float jumpVelocity = 4.5f;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public float mouseSensitivity = 15f;
	public float totalPitch;
	private Vector3 syncPosition = new Vector3();
	private Vector3 syncRotation = new Vector3();
	private float headRotation;
	[Export] Camera3D camera;
	[Export] CsgMesh3D playerMesh;
	double deltaTime;


    public override void _Ready()
    {
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		if(GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
		{
			camera.QueueFree();
		}
		else
		{
			playerMesh.Hide();
		}
	}

    public override void _EnterTree()
    {
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
   	}

    public override void _PhysicsProcess(double delta)
	{
		if(GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			deltaTime = delta;
			Vector3 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
				velocity.Y -= gravity;

			// Handle Jump.
			if (Input.IsActionJustPressed("jump") && IsOnFloor())
				velocity.Y = jumpVelocity;

			// Get the input direction and handle the movement/deceleration.
			// As good practice, you should replace UI actions with custom gameplay actions.
			Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * speed;
				velocity.Z = direction.Z * speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
			}

			Velocity = velocity;
			MoveAndSlide();
		}
	}

    public override void _UnhandledInput(InputEvent @event)
	{
		if(GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			if(@event is InputEventMouseMotion mouseMotionEvent)
			{
				Vector2 mouseVector = mouseMotionEvent.Relative * mouseSensitivity * (float)deltaTime;
				float yaw = mouseVector.X;
				float pitch = mouseVector.Y;
			
				pitch = Mathf.Clamp(pitch, -90 - totalPitch, 90 - totalPitch);
				totalPitch += pitch;

				//Rotate the player around Y axis
				RotateY(Mathf.DegToRad(-yaw));
				//Rotate the camera around X axis
				camera.RotateX(Mathf.DegToRad(-pitch));
			}
		}
	}
}