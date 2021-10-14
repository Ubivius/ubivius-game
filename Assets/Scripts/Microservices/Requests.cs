using UnityEngine;
using UnityEditor;

namespace ubv.microservices
{
    public abstract class MicroserviceRequest
    {
        public abstract string URL();
    }

    public abstract class GetMicroserviceRequest : MicroserviceRequest  { }

    public abstract class PutMicroserviceRequest : MicroserviceRequest
    {
        public abstract string JSONString();
    }

    public abstract class DeleteMicroserviceRequest : MicroserviceRequest { }


    public abstract class PostMicroserviceRequest : MicroserviceRequest
    {
        public abstract string JSONString();
    }
    
}
