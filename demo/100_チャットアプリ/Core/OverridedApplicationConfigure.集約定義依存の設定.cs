using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;
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

    // シーケンスの定義。
    // SQLiteにはシーケンスが無いので、アノテーションで AUTO_INCREMENT を指定する。
    protected override void ConfigureSequenceMember(ModelBuilder modelBuilder, EntityTypeBuilder entity, PropertyBuilder<int?> property, string sequenceName) {
        property.HasAnnotation("Sqlite:Autoincrement", true);
    }

}
