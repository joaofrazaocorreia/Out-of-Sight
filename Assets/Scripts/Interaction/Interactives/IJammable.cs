namespace Interaction
{
    public interface IJammable
    {
        public bool Jammed {get;}
        
        void ToggleJammed();
    }
}