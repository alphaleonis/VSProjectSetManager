using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr
{
   public static class ServiceProviderExtensions
   {
      public static TService GetService<TRegistration, TService>(this IServiceProvider serviceProvider) where TService : class
      {
         return serviceProvider.GetService(typeof(TRegistration)) as TService;
      }

   }
}
