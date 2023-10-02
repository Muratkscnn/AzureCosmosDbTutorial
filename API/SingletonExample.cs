using System;

namespace WebApplication4
{
    public class SingletonExample
    {
        private readonly Guid _instanceId;

        public SingletonExample()
        {
            _instanceId = Guid.NewGuid();
        }

        public string DoSomething()
        {
            return $"Instance ID: {_instanceId}";
        }
    }
}