namespace Interaction
{
    public class FuseBox : IJammable
    {
        public bool Jammed {get => _jammed;}
        private bool _jammed;
        private bool working = true;
        public void ToggleJammed()
        {
            _jammed = !_jammed;
            if (Jammed && working)
            {
                working = false;
                AlertCameraOperator();
            }
        }

        private void AlertCameraOperator()
        {
            
        }
    }
}