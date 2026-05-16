using Nijo.Core;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebServer {
    /// <summary>
    /// ASP.NET Core のコントローラーの抽象
    /// </summary>
    public class Controller {
        internal Controller(Aggregate aggregate) {
            _physicalName = aggregate.PhysicalName;
            _apiEndpoint = aggregate.Options.Handler == NijoCodeGenerator.Models.CommandModel.Key
                ? Models.CommandModelFeatures.CommandController.SUBDOMAIN
                : aggregate.Options.LatinName?.ToKebabCase() ?? aggregate.PhysicalName;
        }

        private readonly string _physicalName;

        public string ClassName => $"{_physicalName}Controller";
        private readonly string _apiEndpoint;

        internal const string SEARCH_ACTION_NAME = "list";
        internal const string CREATE_ACTION_NAME = "create";
        internal const string UPDATE_ACTION_NAME = "update";
        internal const string DELETE_ACTION_NAME = "delete";
        internal const string FIND_ACTION_NAME = "detail";

        internal const string SUBDOMAIN = "api";

        internal string SubDomain => $"{SUBDOMAIN}/{_apiEndpoint}";
    }
}
