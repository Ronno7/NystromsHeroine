namespace HeroineFSMDemo
{
    public sealed class JumpingState : IHeroineState
    {
        public string Name => "Jumping";

        public void Enter(HeroineController h)
        {
            h.ConfigureMovement(h.walkSpeed, h.gravity);
        }

        public void HandleInput(HeroineController h)
        {
            if (h.DuckPressed)
                h.ChangeState(h.Diving);
            else if (h.JumpPressed && h.DoubleJumpAvailable)
            {
                h.Jump();
                h.DoubleJumpAvailable = false;
            }
        }

        public void Tick(HeroineController h)
        {
            // Jumping covers both rising and falling until contact with the floor.
            if (h.IsGrounded && h.VerticalSpeed <= 0f)
                h.ChangeState(h.Standing);
        }

        public void Exit(HeroineController h) { }
    }
}
