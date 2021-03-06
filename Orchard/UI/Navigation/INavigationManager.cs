﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Orchard.UI.Navigation
{
    public interface INavigationManager : IDependency
    {
        IEnumerable<MenuItem> BuildMenu(string menuName);
        IEnumerable<string> BuildImageSets(string menuName);
        string GetUrl(string menuItemUrl, RouteValueDictionary routeValueDictionary);
    }
}
