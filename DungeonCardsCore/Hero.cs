namespace Game
{
    public class Hero
    {
        public int Weapon { get; private set; }
        public int Gold { get; private set; }

        public void AddGold(int goldAmount)
        {
            if (goldAmount < 1)
            {
                return;
            }

            Gold += goldAmount;
        }

        public void PickupWeapon(int weaponDmg)
        {
            if (weaponDmg < 0)
            {
                return;
            }

            Weapon = weaponDmg;
        }

        public bool UseWeapon(int weaponDeduction)
        {
            if (weaponDeduction > Weapon)
            {
                return false;
            }

            Weapon -= weaponDeduction;
            return true;
        }
    }
}
