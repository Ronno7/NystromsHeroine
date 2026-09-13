namespace HeroineFSMDemo
{
    // Every state follows this same contract.
    public interface IHeroineState
    {
        string Name { get; }
        void Enter(HeroineController heroine);
        void HandleInput(HeroineController heroine);
        void Tick(HeroineController heroine);
        void Exit(HeroineController heroine);
    }
}
