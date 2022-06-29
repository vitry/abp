namespace Volo.Abp.EasyNetQ;

public class AbpEasyNetQOptions
{
    public AbpEasyNetQOptions()
    {
        Buses = new EasyNetQBuses();
    }

    public EasyNetQBuses Buses { get; set; }
}