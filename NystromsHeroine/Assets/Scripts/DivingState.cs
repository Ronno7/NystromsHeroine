namespace HeroineFSMDemo
{
    public sealed class DivingState : IHeroineState
    {
        public string Name => "Diving";

        public void Enter(HeroineController h)
        {
            // Begin moving down immediately, then keep accelerating toward the floor.
            h.ConfigureMovement(0f, h.diveAcceleration);
            h.BeginDive();
        }

        public void HandleInput(HeroineController h) { }

        public void Tick(HeroineController h)
        {
            if (h.IsGrounded && h.VerticalSpeed <= 0f)
                h.ChangeState(h.Standing);
        }

        public void Exit(HeroineController h) { }
    }
}
