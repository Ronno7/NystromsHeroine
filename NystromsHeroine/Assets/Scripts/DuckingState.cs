namespace HeroineFSMDemo
{
    public sealed class DuckingState : IHeroineState
    {
        public string Name => "Ducking";

        public void Enter(HeroineController h)
        {
            h.ConfigureMovement(0f, h.gravity);
            h.SetCrouching(true);
        }

        public void HandleInput(HeroineController h)
        {
            // As in the original FSM, release duck before jumping.
            if (!h.DuckHeld)
                h.ChangeState(h.Standing);
        }

        public void Tick(HeroineController h)
        {
            if (!h.IsGrounded)
                h.ChangeState(h.Jumping);
        }

        public void Exit(HeroineController h)
        {
            h.SetCrouching(false);
        }
    }
}
