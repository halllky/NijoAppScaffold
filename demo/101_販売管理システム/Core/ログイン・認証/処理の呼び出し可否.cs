using MyApp.Core.Authorization;
using 機能公開レベル = MyApp.NijoAttr.OperationAccessLevelAttribute.E_Value;

namespace MyApp {
    partial class OverridedApplicationService {

        /// <summary>
        /// すべてのクエリとコマンドの関門。
        /// nijo.xml で処理ごとに宣言された「機能公開レベル」と「機能利用可能権限」をもとに、
        /// ログインユーザーがその処理を呼び出してよいかを、処理の本体より前に確かめる。
        /// 呼び出し元（Web、バッチ、単体テスト）を問わず、処理の公開メソッドを呼ぶと必ずここを通る。
        /// </summary>
        /// <exception cref="OperationForbiddenException">呼び出してよくない場合</exception>
        protected override Task<TReturnValue?> InterceptAsync<TParameter, TReturnValue, TMessages>(
            Operation<OperationContext, TParameter, TReturnValue, TMessages> operation,
            TParameter parameter,
            TMessages messages,
            OperationContext context,
            Func<Task<TReturnValue?>> invokeAsync) where TReturnValue : default {

            if (!operation.Metadata.CanBeInvokedBy(LoginUser)) {
                throw new OperationForbiddenException(operation.Metadata);
            }
            return invokeAsync();
        }
    }
}

namespace MyApp.Core.Authorization {

    /// <summary>
    /// 処理の呼び出し可否の判定
    /// </summary>
    public static class OperationMetadataExtension {
        /// <summary>
        /// 引数のログインユーザーがこの処理を呼び出してよいかを返す。
        /// 「内部処理のみ」の処理は画面から呼び出せないよう WebAPI として公開しないので、ここでは呼び出しを認める。
        /// </summary>
        /// <param name="loginUser">ログインユーザー。ログインしていない場合は null</param>
        public static bool CanBeInvokedBy(this OperationMetadata metadata, LoginUser? loginUser) {
            return metadata.OperationAccessLevel switch {
                機能公開レベル.ログイン不要 => true,
                機能公開レベル.内部処理のみ => true,
                機能公開レベル.ログインユーザー全員 => loginUser != null,
                機能公開レベル.権限で指定 => loginUser != null && metadata.OperationAccessRole.Any(loginUser.Has権限),
                _ => throw new InvalidOperationException($"不明な値です: {metadata.OperationAccessLevel}"),
            };
        }
    }

    /// <summary>
    /// ログインユーザーが、呼び出してよくない処理を呼び出そうとした。
    /// </summary>
    public class OperationForbiddenException(OperationMetadata metadata)
        : Exception($"「{metadata.DisplayName}」を実行する権限がありません。") {
    }
}
