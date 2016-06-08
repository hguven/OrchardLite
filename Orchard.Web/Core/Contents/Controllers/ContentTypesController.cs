﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Services;
using Orchard.Core.Contents.ViewModels;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Contents.Controllers
{
    [Admin, ValidateInput(false)]
    public class ContentTypesController : Controller, IUpdateModel
    {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly Lazy<IEnumerable<IShellSettingsManagerEventHandler>> _settingsManagerEventHandlers;
        private readonly ShellSettings _settings;

        public ContentTypesController(
            IOrchardServices orchardServices,
            IContentDefinitionService contentDefinitionService,
            IContentDefinitionManager contentDefinitionManager,
            Lazy<IEnumerable<IShellSettingsManagerEventHandler>> settingsManagerEventHandlers,
            ShellSettings settings)
        {
            Services = orchardServices;
            _contentDefinitionService = contentDefinitionService;
            _contentDefinitionManager = contentDefinitionManager;
            _settingsManagerEventHandlers = settingsManagerEventHandlers;
            _settings = settings;
        }

        public IOrchardServices Services { get; private set; }

        public ILogger Logger { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            if (!Services.Authorizer.Authorize(Permissions.ViewContentTypes, "Not allowed to view content types."))
                return new HttpUnauthorizedResult();

            return View("List", new ListContentTypesViewModel
            {
                Types = _contentDefinitionService.GetTypes()
            });
        }

        public ActionResult Create(string suggestion)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, "Not allowed to create a content type."))
                return new HttpUnauthorizedResult();

            return View(new CreateTypeViewModel { DisplayName = suggestion, Name = suggestion.ToSafeName() });
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(CreateTypeViewModel viewModel)
        {
            //if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to create a content type.")))
            //    return new HttpUnauthorizedResult();

            viewModel.DisplayName = viewModel.DisplayName ?? String.Empty;
            viewModel.Name = viewModel.Name ?? String.Empty;

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", "The Display Name name can't be empty.");
            }

            if (String.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError("Name", "The Content Type Id can't be empty.");
            }

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "A type with the same Id already exists.");
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Name) && !viewModel.Name[0].IsLetter())
            {
                ModelState.AddModelError("Name", "The technical name must start with a letter.");
            }

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.DisplayName.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("DisplayName", "A type with the same Display Name already exists.");
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            var contentTypeDefinition = _contentDefinitionService.AddType(viewModel.Name, viewModel.DisplayName);

            // adds CommonPart by default
            _contentDefinitionService.AddPartToType("CommonPart", viewModel.Name);

            var typeViewModel = new EditTypeViewModel(contentTypeDefinition);

            Services.Notifier.Information(string.Format("The \"{0}\" content type has been created.", typeViewModel.DisplayName));

            return RedirectToAction("AddPartsTo", new { id = typeViewModel.Name });
        }

        public ActionResult ListParts()
        {
            return View(new ListContentPartsViewModel
            {
                // only user-defined parts (not code as they are not configurable)
                Parts = _contentDefinitionService.GetParts(true/*metadataPartsOnly*/)
            }); 
        }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return base.TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, Localization.LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}