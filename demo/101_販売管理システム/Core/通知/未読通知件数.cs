using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {
    protected override async Task<未読通知件数ReturnValueDisplayData?> OnExecute未読通知件数Async(MessageSetter messages, OperationContext context) {
        var count = await Filter自分宛て(DbContext.通知DbSet)
            .Where(n => n.既読日時 == null)
            .CountAsync();
        return new() { 件数 = count };
    }
}
