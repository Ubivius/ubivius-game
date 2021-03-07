using UnityEngine;
using System.Collections;
using System.Net.Http;

namespace ubv.http
{
    public abstract class HTTPReceiver 
    {
        protected virtual void OnGet(string response) { }
        protected virtual void OnGet(byte[] response) { }
        
        protected virtual void OnPost(HttpResponseMessage response) { }
        protected virtual void OnPut(HttpResponseMessage response) { }
        protected virtual void OnDelete(HttpResponseMessage response) { }
    }
}
