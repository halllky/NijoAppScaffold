import React from "react";

/**
 * セルエディタのためにIME開閉状態を管理するフック
 */
export function useImeOpened(containerRef: React.RefObject<HTMLElement | null>): boolean {

  const [isImeOpened, setIsImeOpened] = React.useState(false);

  React.useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const handleCompositionStart = () => {
      setIsImeOpened(true);
    };

    const handleCompositionEnd = () => {
      setIsImeOpened(false);
    };

    container.addEventListener("compositionstart", handleCompositionStart);
    container.addEventListener("compositionend", handleCompositionEnd);

    return () => {
      container.removeEventListener("compositionstart", handleCompositionStart);
      container.removeEventListener("compositionend", handleCompositionEnd);
    };
  }, [containerRef]);

  return isImeOpened;
}
