public class PowerupNode
{
    public PowerupBase Power;
    public PowerupNode Next;

    public PowerupNode(PowerupBase power)
    {
        Power = power;
    }
}