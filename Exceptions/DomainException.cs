using System;

namespace Topluluk_Yonetim.MVC.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }
}


