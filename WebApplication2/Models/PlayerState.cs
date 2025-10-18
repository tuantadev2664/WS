namespace MiniBattleship.Models
{
    public class PlayerState
    {
        public Board Own { get; set; }
        public Board OppView { get; set; }
        public string Name { get; set; }


        public PlayerState(Board own, Board oppView, string name)
        {
            Own = own;
            OppView = oppView;
            Name = name;
        }
    }
}
