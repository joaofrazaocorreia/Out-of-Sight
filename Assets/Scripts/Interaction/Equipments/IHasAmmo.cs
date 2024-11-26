namespace Interaction.Equipments
{
    public interface IHasAmmo
    {
        public int MaxAmmo { get; set; }
        public int CurrentAmmo { get; set; }
    }
}