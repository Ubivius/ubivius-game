using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace ubv.microservices
{
    public abstract class MicroserviceRequest
    {
        public abstract string URL();
        public readonly UnityAction<string> FailureCallback;

        public MicroserviceRequest(UnityAction<string> failureCallback)
        {
            FailureCallback = failureCallback;
        }
    }

    public abstract class GetMicroserviceRequest : MicroserviceRequest
    {
        public GetMicroserviceRequest(UnityAction<string> failureCallback = default) : base(failureCallback) { }
    }

    public abstract class PutMicroserviceRequest : MicroserviceRequest
    {
        public abstract string JSONString();
        public PutMicroserviceRequest(UnityAction<string> failureCallback = default) : base(failureCallback) { }
    }

    public abstract class DeleteMicroserviceRequest : MicroserviceRequest
    {
        public DeleteMicroserviceRequest(UnityAction<string> failureCallback = default) : base(failureCallback) { }
    }


    public abstract class PostMicroserviceRequest : MicroserviceRequest
    {
        public abstract string JSONString();
        public PostMicroserviceRequest(UnityAction<string> failureCallback = default) : base(failureCallback) { }
    }
    
}
