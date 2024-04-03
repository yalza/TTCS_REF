public enum PlayerState : byte
{
    Idle = 0,
    Walking = 1,
    Running = 2,
    Crouching = 3,
    Jumping = 4,
    Climbing = 5,
    Sliding = 6,
    Dropping = 7,
    Gliding = 8,
    InVehicle = 9,
    Stealth = 10
}

public enum PlayerFPState : byte
{
    Idle = 0,
    Firing = 1,
    Reloading = 2,
    Aiming = 3,
    Running = 4,
    FireAiming = 9,
}

public enum PlayerRunToAimBehave
{
    BlockAim = 0,
    StopRunning = 1,
    AimWhileRunning = 2,
}