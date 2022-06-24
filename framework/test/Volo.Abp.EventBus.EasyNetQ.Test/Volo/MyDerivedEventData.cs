namespace Volo.Abp.EventBus.EasyNetQ;

public class MyDerivedEventData : MySimpleEventData
{
    public MyDerivedEventData(int value)
        : base(value)
    {
    }
}