using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyApp;

partial class OverridedApplicationConfigure {

    public override bool ValidateIf半角数字および半角ハイフンのみ(string? value) {
        if (string.IsNullOrEmpty(value)) {
            return true;
        }
        foreach (char c in value) {
            if ((c < '0' || c > '9') && c != '-') {
                return false;
            }
        }
        return true;
    }

}
