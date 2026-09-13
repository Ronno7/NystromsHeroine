namespace HeroineFSMDemo
{
    public sealed class StandingState : IHeroineState
    {
        public string Name => "Standing";

        public void Enter(HeroineController h)
        {
            h.ConfigureMovement(h.walkSpeed, h.gravity);
            h.DoubleJumpAvailable = true;
        }

        public void HandleInput(HeroineController h)
        {
            // Sprint is a feature of standing, including grounded movement.
            h.ConfigureMovement(h.SprintHeld ? h.sprintSpeed : h.walkSpeed, h.gravity);

            if (h.DuckHeld)
                h.ChangeState(h.Ducking);
            else if (h.JumpPressed && h.IsGrounded)
            {
                h.Jump();
                h.ChangeState(h.Jumping);
            }
        }

        public void Tick(HeroineController h)
        {
            if (!h.IsGrounded)
                h.ChangeState(h.Jumping);
        }

        public void Exit(HeroineController h) { }
    }
}
