﻿using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public interface IUpdateModel
    {
        bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class;
        void AddModelError(string key, LocalizedString errorMessage);
    }
}
