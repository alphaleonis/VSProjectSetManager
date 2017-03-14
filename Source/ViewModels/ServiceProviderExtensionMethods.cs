using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr
{
   public static class ServiceProviderExtensionMethods
   {
      public static T GetService<T>(this IServiceProvider provider)
      {
         return (T)provider.GetService(typeof(T));
      }

      public static T RequireService<T>(this IServiceProvider provider)
      {
         T service = provider.GetService<T>();
         if (service == null)
            throw new NotSupportedException(String.Format("No service of type {0} is registered in this service provider.", typeof(T).Name));
         return service;
      }
   }
}
